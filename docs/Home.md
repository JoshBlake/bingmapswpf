| [Home](Home) | [Getting Started](Getting-Started) | [Features and RoadMap](Features-and-RoadMap) | [Project Structure](Project-Structure) | [Screenshots and Video](Screenshots-and-Video) | [Application list](Application-list) |
# **Project Summary**
This control provides a WPF interface for the Bing Maps 3D control, complete with data binding for camera control and WPF-based pushpins.  All Win32 restrictions (air space control, no rotation or visual brush) are eliminated. Microsoft Surface and Win 7 Touch are also supported.
# **Project Details**
Bing Maps 3D (formerly Virtual Earth 3D) has many applications, but until recently has only been practical on the web with a javascript interface.  WPF applications could not use it to its full potential without requiring a WPF wizard and some XAML magic due to Win32 interop limitations.  

The solution is here: **InfoStrat.VE**

Created by [InfoStrat](http://www.InfoStrat.com), this project provides a VEMap control for WPF, SurfaceVEMap control for Microsoft Surface, and Win7TouchVEMap control for Windows 7 Touch.  SurfaceVEMap and Win7TouchVEMap each derive from VEMap and adds multi-touch manipulation support.

For examples of Microsoft Surface solutions we've built with this control go to our [InfoStrat Surface gallery](http://www.InfoStrat.com/home/solutions/Surface/SurfaceSVP.htm).

InfoStrat.VE allows WPF and Microsoft Surface developers to take full advantage of Bing Maps 3D with minimal overhead.  Simply reference the dll, add a single VEMap control to your XAML, and you have a map!  The control eliminates the Win32 Interop restrictions, so you can do everything with this VE control that you could do with any native WPF control, including:

* Overlay items  _(no more transparent windows!)_
* Rotate and transform the map within the interface _(no more boring rectangles, bring on the 360 degree interfaces!)_
* Use the map within a Visual Brush _(you know you want faded reflections!)_

Head over to [Getting Started](Getting-Started) to ... you know, get started!
