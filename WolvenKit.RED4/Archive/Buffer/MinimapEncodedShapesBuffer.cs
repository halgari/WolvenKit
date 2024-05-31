using WolvenKit.Core.Attributes;
using WolvenKit.RED4.Types;

namespace WolvenKit.RED4.Archive.Buffer;

public class MinimapEncodedShapesBuffer : IParseableBuffer
{
    public IRedType? Data { get; } = null;

    public CArray<MinimapEncodedShapesBuffer_ShapeAddress> ShapeAddresses { get; set; } = [];
    public CArray<MinimapEncodedShapesBuffer_EncodedShapeInfo> EncodedShapeInfos { get; set; } = [];
    public CArray<Vector3> LocalsToModels { get; set; } = [];
    public CArray<MinimapEncodedShapesBuffer_Bucket> Buckets { get; set; } = [];
    public CArray<Box> BucketsBounds { get; set; } = [];
    public CArray<MinimapEncodedShapesBuffer_ShapeIndex> Indices { get; set; } = [];
    public CArray<MinimapEncodedShapesBuffer_Box> LocalBounds { get; set; } = [];
    public CArray<Vector2> Vertices { get; set; } = [];
    public CArray<CUInt16> VertexIndices { get; set; } = [];
    public CArray<CUInt64> Owners { get; set; } = [];
}

public class MinimapEncodedShapesBuffer_ShapeAddress : IRedClass
{
    public CUInt32 VertexIndex { get; set; }
    public CUInt16 Count { get; set; }
    public CUInt16 BoundsIndex { get; set; }
}

public class MinimapEncodedShapesBuffer_EncodedShapeInfo : IRedClass
{
    public CUInt16 OwnerIndex { get; set; }
    public CUInt8 ShapeType { get; set; }
    public CUInt8 TypeFlag { get; set; }
}

public class MinimapEncodedShapesBuffer_Bucket : IRedClass
{
    public CUInt16 Begin { get; set; }
    public CUInt16 Count { get; set; }
}

public class MinimapEncodedShapesBuffer_Box : IRedClass
{
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }
}

public class MinimapEncodedShapesBuffer_ShapeIndex : IRedClass
{
    public CUInt16 BorderIndex { get; set; }
    public CUInt16 FillIndex { get; set; }
    public CUInt16 NumBorderIndices { get; set; }
    public CUInt16 NumFillIndices { get; set; }
    public CUInt8 PaddingUnk { get; set; }

    internal MinimapEncodedShapesBuffer_ShapeIndexStruct ToStruct() =>
        new()
        {
            BorderIndex = BorderIndex,
            FillIndex = FillIndex,
            NumBorderIndices = NumBorderIndices,
            NumFillIndices = NumFillIndices,
            PaddingUnk = PaddingUnk
        };

    internal static MinimapEncodedShapesBuffer_ShapeIndex FromStruct(MinimapEncodedShapesBuffer_ShapeIndexStruct data) =>
        new()
        {
            BorderIndex = data.BorderIndex,
            FillIndex = data.FillIndex,
            NumBorderIndices = data.NumBorderIndices,
            NumFillIndices = data.NumFillIndices,
            PaddingUnk = data.PaddingUnk
        };
}

internal struct MinimapEncodedShapesBuffer_ShapeIndexStruct
{
    [BitfieldLength(12)] public ushort BorderIndex;
    [BitfieldLength(12)] public ushort FillIndex;
    [BitfieldLength(16)] public ushort NumBorderIndices;
    [BitfieldLength(16)] public ushort NumFillIndices;
    [BitfieldLength(8)] public byte PaddingUnk;
}