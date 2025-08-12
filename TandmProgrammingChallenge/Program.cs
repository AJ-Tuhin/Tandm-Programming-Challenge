using NetTopologySuite.Geometries;
using System.Numerics;

var ceiling = new List<Vector3>
{
    new(97500f, 34000f, 2500f),
    new(85647.67f, 43193.61f, 2500f),
    new(91776.75f, 51095.16f, 2500f),
    new(103629.07f, 41901.55f, 2500f)
};

var pipes = new List<(Vector3 A, Vector3 B)>
{
    (new(98242.11f, 36588.29f, 3000f), new(87970.10f, 44556.09f, 3500f)),
    (new(99774.38f, 38563.68f, 3500f), new(89502.37f, 46531.47f, 3000f)),
    (new(101306.65f, 40539.07f, 3000f), new(91034.63f, 48506.86f, 3000f))
};

const double spacing = 2500.0;
const float ceilingZ = 2500f;

var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();
var ceilingBoundary = ceiling.Select(v => new Coordinate((double)v.X, (double)v.Y)).ToList();
ceilingBoundary.Add(ceilingBoundary[0]); 
var shell = new LinearRing(ceilingBoundary.ToArray());
var roomPoly = new Polygon(shell);

var buffered = roomPoly.Buffer(-spacing);

Polygon offsetPolygon = null;
if (buffered is Polygon p)
{
    offsetPolygon = p;
}

var offsetBoundary = offsetPolygon.ExteriorRing.Coordinates.Select(c => new Vector2((float)c.X, (float)c.Y)).ToList();

var env = offsetPolygon.EnvelopeInternal;
var minX = env.MinX;
var maxX = env.MaxX;
var minY = env.MinY;
var maxY = env.MaxY;

var startX = Math.Ceiling(minX / spacing) * spacing;
var startY = Math.Ceiling(minY / spacing) * spacing;

var sprinklers = new List<Vector3>();

for (double x = startX; x <= maxX; x += spacing)
{
    for (double y = startY; y <= maxY; y += spacing)
    {
        var pt = geometryFactory.CreatePoint(new Coordinate(x, y));
        if (offsetPolygon.Contains(pt))
        {
            sprinklers.Add(new Vector3((float)x, (float)y, ceilingZ));
        }
    }
}
 
var results = new List<(Vector3 Sprinkler, int PipeIndex, Vector3 Connection)>();
for (int i = 0; i < sprinklers.Count; i++)
{
    var sprinklerPosition = sprinklers[i];
    double closestDistance = double.MaxValue;
    int closestPipeIndex = -1;
    Vector3 closestConnection = new();

    for (int pipeIndex = 0; pipeIndex < pipes.Count; pipeIndex++)
    {
        var (PipeStart, PipeEnd) = pipes[pipeIndex];
        var conn = ClosestPointOnSegment3D(PipeStart, PipeEnd, sprinklerPosition);
        var distanceToSprinkler = Vector3.Distance(conn, sprinklerPosition);
        if (distanceToSprinkler < closestDistance)
        {
            closestDistance = distanceToSprinkler;
            closestPipeIndex = pipeIndex;
            closestConnection = conn;
        }
    }

    results.Add((sprinklerPosition, closestPipeIndex, closestConnection));
}


Console.WriteLine($"Total sprinklers: {results.Count}");

for (int i = 0; i < results.Count; i++)
{
    var sprinklerInfo = results[i];
    Console.WriteLine($"Sprinkler {i + 1}: ({sprinklerInfo.Sprinkler.X:F2}, {sprinklerInfo.Sprinkler.Y:F2}, {sprinklerInfo.Sprinkler.Z:F2})");
    Console.WriteLine($"Connection: ({sprinklerInfo.Connection.X:F2}, {sprinklerInfo.Connection.Y:F2}, {sprinklerInfo.Connection.Z:F2})");
}

static Vector3 ClosestPointOnSegment3D(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point)
{
    var segmentVector = segmentEnd - segmentStart;
    var startToPointVector = point - segmentStart;
    float segmentLengthSquared = Vector3.Dot(segmentVector, segmentVector);

    if (segmentLengthSquared == 0f)
    {
        return segmentStart;
    }

    float projection = Vector3.Dot(startToPointVector, segmentVector) / segmentLengthSquared;
    projection = Math.Clamp(projection, 0f, 1f);

    return segmentStart + projection * segmentVector;
}
