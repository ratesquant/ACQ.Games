using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ACQ.Core
{
    public class ArithmeticCoder
    {
        // constants to split the number space of 32 bit integers
        // most significant bit kept free to prevent overflows
        public const uint g_FirstQuarter = 0x20000000;
        public const uint g_Half = 0x40000000;
        public const uint g_ThirdQuarter = 0x60000000;

        protected byte mBitBuffer;
        protected byte mBitCount;
        protected Stream mStream;
        protected uint mLow;
        protected uint mHigh;
        protected uint mStep;
        protected uint mScale;
        protected uint mBuffer;

        public ArithmeticCoder()
        {
            Reset();
        }

        public void Reset()
        {
            mBitBuffer = 0;
            mBitCount = 0;

            mLow = 0;
            mHigh = 0x7FFFFFFF; // just work with least significant 31 bits
            mScale = 0;
            mStep = 0;

            mBuffer = 0;
        }

        public void SetStream(Stream stream)
        {
            mStream = stream;
        }

        public void Encode(uint low_count, uint high_count, uint total)
        // total < 2^29
        {
            // partition number space into single steps
            mStep = (mHigh - mLow + 1) / total; // interval open at the top => +1

            // update upper bound
            mHigh = mLow + mStep * high_count - 1; // interval open at the top => -1

            // update lower bound
            mLow = mLow + mStep * low_count;

            // Output and Expand, Subdivide in ModelOrder0.Encode
            while ((mHigh < g_Half) || (mLow >= g_Half))
            {
                if (mHigh < g_Half)
                {
                    SetBit(0);
                    mLow = mLow * 2;
                    mHigh = mHigh * 2 + 1;

                    // Reseting recalls set by follow actions
                    for (; mScale > 0; mScale--)
                        SetBit(1);
                }
                else if (mLow >= g_Half)
                {
                    SetBit(1);
                    mLow = (mLow - g_Half) * 2;
                    mHigh = (mHigh - g_Half) * 2 + 1;

                    // Reseting recalls set by follow actions
                    for (; mScale > 0; mScale--)
                        SetBit(0);

                }
            }

            // Set an recall to be applied later
            while ((g_FirstQuarter <= mLow) && (mHigh < g_ThirdQuarter))
            {
                // keep necessary mappings in mind
                mScale++;
                mLow = (mLow - g_FirstQuarter) * 2;
                mHigh = (mHigh - g_FirstQuarter) * 2 + 1;
            }
        }

        public void EncodeFinish()
        {
            // There are two possibilities of how mLow and mHigh can be distributed,
            // which means that two bits are enough to distinguish them.

            if (mLow < g_FirstQuarter) // mLow < FirstQuarter < Half <= mHigh
            {
                SetBit(0);

                for (int i = 0; i < mScale + 1; i++) // Reseting recalls in mind
                    SetBit(1);
            }
            else // mLow < Half < ThirdQuarter <= mHigh
            {
                SetBit(1); // zeros added automatically by the decoder; no need to send them
            }

            // empty the output buffer
            SetBitFlush();
        }

        public void DecodeStart()
        {
            // empty the output buffer
            for (int i = 0; i < 31; i++) // just use the 31 least significant bits
                mBuffer = (mBuffer << 1) | GetBit();
        }

        // converts raw stored data to original value
        public uint DecodeTarget(uint total)
        // total < 2^29
        {
            // split number space into single steps
            mStep = (mHigh - mLow + 1) / total;

            // return current value
            return (mBuffer - mLow) / mStep;
        }

        public void Decode(uint low_count, uint high_count)
        {
            // update upper bound
            mHigh = mLow + high_count * mStep - 1;

            // update lower bound
            mLow = mLow + low_count * mStep;

            // Output and Expand, Subdivide in ModelOrder0.Decode, to get synced with Encoder
            while ((mHigh < g_Half) || (mLow >= g_Half))
            {
                if (mHigh < g_Half)
                {
                    mLow = mLow * 2;
                    mHigh = mHigh * 2 + 1;
                    mBuffer = mBuffer * 2 + GetBit();
                }
                else if (mLow >= g_Half)
                {
                    mLow = (mLow - g_Half) * 2;
                    mHigh = (mHigh - g_Half) * 2 + 1;
                    mBuffer = (mBuffer - g_Half) * 2 + GetBit();
                }
                mScale = 0;
            }

            // Set an recall to be applied later
            while ((g_FirstQuarter <= mLow) && (mHigh < g_ThirdQuarter))
            {
                mScale++;
                mLow = (mLow - g_FirstQuarter) * 2;
                mHigh = (mHigh - g_FirstQuarter) * 2 + 1;
                mBuffer = (mBuffer - g_FirstQuarter) * 2 + GetBit();
            }
        }

        protected void SetBit(uint bit)
        {
            // add bit to the buffer
            mBitBuffer = (byte)(((uint)mBitBuffer << 1) | bit);
            mBitCount++;

            if (mBitCount == 8) // buffer full
            {
                // write
                mStream.WriteByte(mBitBuffer);
                mBitCount = 0;
            }

        }

        protected void SetBitFlush()
        {
            // fill buffer with 0 up to the next byte
            while (mBitCount != 0)
            {
                SetBit(0);
            }
        }

        protected uint GetBit()
        {
            if (mBitCount == 0) // buffer empty
            {
                int readInt = mStream.ReadByte();
                if (readInt == -1) // EOF = Is file read completely?
                    mBitBuffer = 0;
                else
                    mBitBuffer = (byte)readInt; // append zeros

                mBitCount = 8;
            }

            // extract bit from buffer
            uint bit = (uint)mBitBuffer >> 7;
            mBitBuffer = (byte)((uint)mBitBuffer << 1);
            mBitCount--;

            return bit;
        }
    }

    public enum ACModeE
    {
        MODE_ENCODE = 0,
        MODE_DECODE = 1
    }

    public abstract class ACModel : ICompressor
    {
        public byte[] Compress(byte[] input)
        {
            byte[] result = null;
            using (MemoryStream target = new MemoryStream())
            {
                Process(new MemoryStream(input), target, ACModeE.MODE_ENCODE);
                result = target.ToArray();
            }
            return result;
        }

        public byte[] Decompress(byte[] input)
        {
            byte[] result = null;
            using (MemoryStream target = new MemoryStream())
            {
                Process(new MemoryStream(input), target, ACModeE.MODE_DECODE);
                result = target.ToArray();
            }
            return result;
        }

        public void Process(Stream source, Stream target, ACModeE mode)
        {
            Init();

            mSource = source;
            mTarget = target;

            if (mode == ACModeE.MODE_ENCODE)
            {
                mAC.SetStream(mTarget);

                Encode();

                mAC.EncodeFinish();
            }
            else
            {
                mAC.SetStream(mSource);

                mAC.DecodeStart();

                Decode();
            }
            mAC.Reset();
        }

        protected abstract void Encode();
        protected abstract void Decode();
        protected abstract void Init();

        protected ArithmeticCoder mAC = new ArithmeticCoder();
        protected Stream mSource;
        protected Stream mTarget;
    }

    public class ACModelOrder0 : ACModel
    {
        protected uint[] mCumCount = new uint[257];
        protected uint mTotal;

        public ACModelOrder0()
        {           
        }

        protected override void Init()
        {
            mTotal = 257; // 256 + escape symbol for termination

            // initialize probabilities with 1
            for (uint i = 0; i < 257; i++)
                mCumCount[i] = 1;
        }

        protected override void Encode()
        {
            int readInt = 0;
            while (readInt != -1) // EOF
            {
                byte symbol;
                // read symbol
                readInt = mSource.ReadByte();
                symbol = (byte)readInt;

                if (readInt != -1) // EOF
                {
                    uint low_count = 0;

                    // cumulate frequencies to find the appropriate subinterval in subdivide action
                    for (byte j = 0; j < symbol; j++)
                    {
                        low_count += mCumCount[j];
                    }

                    // encode symbol
                    mAC.Encode(low_count, low_count + mCumCount[symbol], mTotal);

                    // update model => adaptive encoding model
                    mCumCount[symbol]++;
                    mTotal++;
                }
            }

            // write escape symbol ($ in docs) for termination
            mAC.Encode(mTotal - 1, mTotal, mTotal);
        }

        protected override void Decode()
        {
            uint symbol; // uint instead of byte because it must be able to contain 256 as terminator

            do
            {
                uint value;

                // read value
                value = mAC.DecodeTarget(mTotal);

                uint low_count = 0;
                // determine symbol
                for (symbol = 0; low_count + mCumCount[symbol] <= value; symbol++)
                    low_count += mCumCount[symbol];

                // Write symbol, if it was not terminator
                if (symbol < 256)
                    mTarget.WriteByte((byte)symbol);

                // adapt decoder
                mAC.Decode(low_count, low_count + mCumCount[symbol]);

                // update model
                mCumCount[symbol]++;
                mTotal++;
            } while (symbol != 256); // until terminator
        }

        public static void Test()
        {
            ACModel model = new ACModelOrder0();

            string text = @"C#(pronounced see sharp) is a multi-paradigm programming language encompassing strong typing, imperative, declarative, functional, generic, object-oriented (class-based), and component-oriented programming disciplines. It was developed by Microsoft within its .NET initiative and later approved as a standard by Ecma (ECMA-334) and ISO (ISO/IEC 23270:2006). C# is one of the programming languages designed for the Common Language Infrastructure.";

            MemoryStream sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            MemoryStream targetStream = new MemoryStream();
            MemoryStream decompStream = new MemoryStream();
           
            model.Process(sourceStream, targetStream, ACModeE.MODE_ENCODE);

            targetStream.Close();
            byte[] compressed = targetStream.ToArray();
           
            model.Process(new MemoryStream(compressed), decompStream, ACModeE.MODE_DECODE);

            decompStream.Close();
            byte[] decompressed = decompStream.ToArray();

            string output = Encoding.UTF8.GetString(decompressed);

            Console.WriteLine("Decompression {0}: {1}", output.Equals(text), output);              
        }
    }
}
