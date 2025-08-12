1. Create Polygon from Ceiling Points 
  • We use only x and y coordinates (ignore z for polygon). 
  • We close the polygon by repeating the first point at the end. 
2. Offset Polygon (Wall Clearance) 
  • You need sprinklers at least 2500 mm away from the walls. 
  • So, we shrink the polygon inward by 2500 mm using NetTopologySuite 
3. Generate Grid Points inside Offset Polygon 
  • Create a grid starting from startX, startY inside the bounding box of offset polygon. 
  • Step size = 2500 mm (spacing). 
4. Check Each Grid Point - Is It Inside Polygon? 
  • For each grid point, check if it lies inside the offset polygon. 
  • If yes, add it to sprinkler list. 
5. Find Nearest Pipe & Connection Point for Each Sprinkler 
  • Each sprinkler must connect to the closest point on the nearest pipe segment. 
  • We calculate distance from sprinkler to each pipe segment. 
  • Choose the pipe with minimum distance. 
6. Output 
  • Show the sprinkle count. 
  • Show the sprinkler coordinates (x,y,z). 
  • Show connection coordinate.
