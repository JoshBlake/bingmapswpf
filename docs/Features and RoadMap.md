| [Home](Home) | [Getting Started](Getting-Started) | [Features and RoadMap](Features-and-RoadMap) | [Project Structure](Project-Structure) | [Screenshots and Video](Screenshots-and-Video) | [Application list](Application-list) |
# Features and RoadMap
## Release 1 (March, 2009)

The current version of InfoStrat.VE supports these features:

**VEMap and SurfaceVEMap:**
* Displays the (rectangular) Bing Maps 3D map
* Use WPF data binding to control various map properties:
	* Latitude and Longitude
	* Altitude
	* Roll, Pitch, and Yaw
	* Map Mode (Aerial, Hybrid, Road)
	* 3D Cursor (WPF only)
	* Show 3D Buildings
* Simple functions allow cinematic FlyTo a location
* Supports children objects such as pure WPF-based VEPushPin and SurfaceVEPushPin objects
	* Child objects automatically track a Latitude and Longitude on the map
	* Map can use VEPushPins through data binding, just like a ListBox or other ItemsControl
	* VEPushPins have optional max/min altitude and parent pushpin for grouping purposes
* You can inherit VEPushPin and SurfaceVEPushPin and create your own custom look and behavior

**SurfaceVEMap only:**
* Multi-touch/gesture manipulations for controlling Latitude and Longitude (pan) and Altitude (zoom)
* Data binding for Pan and Zoom gesture sensitivity (SurfaceVEMap only)

**Sample applications:**
* Demonstrates basic data binding
* Shows simple VEPushPin operations
## Release 2 (January 2010)

Release 2 added a Win7TouchVEMap control, better map movement algorithms, new manipulations for pivoting and tilting, plus a dozen community generated feature requests or bug fixes.

See the discussion thread for full details:
[discussion:79658](discussion_79658)

## December 2011 and beyond
The Bing Maps 3D control upon which this control is based has been discontinued by Microsoft. It still works and the services it uses are still alive (as of February, 2012) but we are no longer actively developing InfoStrat.VE. See these links for more information:
[Bing Maps 3D Discontinued](http://blogs.msdn.com/b/virtualearth3d/archive/2010/11/03/discontinuing-investment-is-such-sweet-sorrow.aspx)
[Discussion thread on CodePlex](http://bingmapswpf.codeplex.com/discussions/248743)


Developers may wish to consider using the new and officially supported Bing Maps WPF Control v1, which is a 2D map:
[Bing Maps WPF Control v1 announcement](http://www.bing.com/community/site_blogs/b/maps/archive/2012/01/12/announcing-the-bing-maps-windows-presentation-foundation-control-v1.aspx)