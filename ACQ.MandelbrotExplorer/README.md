# Mandelbrot Explorer
The C# application is a graphical user interface designed to explore the intricate beauty of the Mandelbrot set. It provides map-like controls for zooming and panning the image, allowing users to navigate and explore the fractal in detail.

The Mandelbrot set is a complex mathematical object that can be represented as a colorful image. The application uses complex numbers and the iterative calculation of the Mandelbrot set to generate the image. Users can zoom in on a particular region of the image by clicking and dragging on the map, and pan around the fractal by dragging the image itself.

The application also includes various settings to customize the display of the fractal, such as changing the color scheme, adjusting the maximum iteration depth, and selecting a specific region of the Mandelbrot set to focus on.

This application allows users to save images of the fractal in 4K resolution. 

![screenshot](https://github.com/ratesquant/ACQ.Games/blob/714c418325eaf1144f2c19904450b3cbb579bbc1/ACQ.MandelbrotExpoler/images/Screenshot2.png?raw=true)

# Controls 
The application provides intuitive controls for navigating and exploring the Mandelbrot set including the ability to zoom in up to 1e13 magnification. Here's how the controls work:

1. To zoom in or out (up to 1e13), simply use your mouse wheel. This allows for quick and easy zooming, making it simple to focus in on a particular area of the fractal.

2. For more precise zooming, you can select a specific region to zoom in on by holding down the middle mouse button and dragging your mouse across the desired area. This allows you to zoom in on a specific part of the fractal with greater accuracy and precision.

3. To pan around the Mandelbrot set, simply hold down the left mouse button and drag your mouse in the desired direction. This allows you to explore the fractal and navigate around the image to find interesting patterns and shapes.

4. If you want to reset your zoom level back to the default view, simply double-click the left mouse button. This is a quick and easy way to return to the original view of the fractal and start exploring again.

Overall, these controls provide an intuitive and user-friendly way to explore the Mandelbrot set, allowing users to zoom in, select regions, pan around the fractal, and reset their view with ease.

If the application detects that your computer has a compatible GPU, it will automatically attempt to use it for the calculations required to render the Mandelbrot set (ILGPU (https://ilgpu.net/)). This can significantly speed up the rendering process and provide smoother and more responsive navigation when exploring the fractal.

# Examples 
Example with "All" palette - which uses all standard named C# colors

![screenshot2](https://github.com/ratesquant/ACQ.Games/blob/3263a27f1a3c46fe689ecd3ae85b8250f7cba984/ACQ.MandelbrotExpoler/images/m_all.png?raw=true)

Example with Cubehelix palette (https://jiffyclub.github.io/palettable/cubehelix/)

![screenshot3](https://github.com/ratesquant/ACQ.Games/blob/3263a27f1a3c46fe689ecd3ae85b8250f7cba984/ACQ.MandelbrotExpoler/images/m_cubehelix.png?raw=true)

Example with jet palette

![screenshot3](https://github.com/ratesquant/ACQ.Games/blob/3263a27f1a3c46fe689ecd3ae85b8250f7cba984/ACQ.MandelbrotExpoler/images/m_jet.png?raw=true)
