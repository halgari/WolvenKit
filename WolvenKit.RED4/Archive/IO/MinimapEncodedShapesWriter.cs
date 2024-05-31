using WolvenKit.Core.Extensions;
using WolvenKit.RED4.Archive.Buffer;
using WolvenKit.RED4.IO;
using WolvenKit.RED4.Types;

namespace WolvenKit.RED4.Archive.IO;

public class MinimapEncodedShapesWriter : Red4Writer
{
    public MinimapEncodedShapesWriter(Stream output) : base(output)
    {
    }

    public void WriteBuffer(MinimapEncodedShapesBuffer data, minimapEncodedShapes parent)
    {
        if (data.ShapeAddresses.Count != data.EncodedShapeInfos.Count ||
            data.ShapeAddresses.Count != data.LocalsToModels.Count ||
            data.ShapeAddresses.Count != data.Indices.Count)
        {
            throw new Exception();
        }

        if (data.Buckets.Count != data.BucketsBounds.Count)
        {
            throw new Exception();
        }

        foreach (var shapeAddress in data.ShapeAddresses)
        {
            Write(shapeAddress.VertexIndex);
            Write(shapeAddress.Count);
            Write(shapeAddress.BoundsIndex);
        }

        foreach (var encodedShapeInfo in data.EncodedShapeInfos)
        {
            Write(encodedShapeInfo.OwnerIndex);
            Write(encodedShapeInfo.ShapeType);
            Write(encodedShapeInfo.TypeFlag);
        }

        foreach (var vector3 in data.LocalsToModels)
        {
            Write(vector3.X);
            Write(vector3.Y);
            Write(vector3.Z);
        }

        foreach (var bucket in data.Buckets)
        {
            Write(bucket.Begin);
            Write(bucket.Count);
        }

        foreach (var bucketsBound in data.BucketsBounds)
        {
            Write(bucketsBound.Min.X);
            Write(bucketsBound.Min.Y);
            Write(bucketsBound.Min.Z);
            Write(bucketsBound.Min.W);

            Write(bucketsBound.Max.X);
            Write(bucketsBound.Max.Y);
            Write(bucketsBound.Max.Z);
            Write(bucketsBound.Max.W);
        }

        foreach (var index in data.Indices)
        {
            BaseStream.WriteBitfield(index.ToStruct());
        }

        foreach (var localBound in data.LocalBounds)
        {
            var (minX, minY, minZ) = QuantizeVec3(localBound.Min);
            var (maxX, maxY, maxZ) = QuantizeVec3(localBound.Max);

            BaseWriter.Write(minX);
            BaseWriter.Write(minY);
            BaseWriter.Write(minZ);

            BaseWriter.Write(maxX);
            BaseWriter.Write(maxY);
            BaseWriter.Write(maxZ);
        }

        foreach (var vertex in data.Vertices)
        {
            var (x, y) = QuantizeVec2(vertex);

            BaseWriter.Write(x);
            BaseWriter.Write(y);
        }

        foreach (var vertexIndex in data.VertexIndices)
        {
            Write(vertexIndex);
        }

        foreach (var owner in data.Owners)
        {
            Write(owner);
        }

        (ushort x, ushort y) QuantizeVec2(Vector2 vector2)
        {
            var q = 1f / 65535f;

            var x = (ushort)((vector2.X - parent.QuantizationBias.X) / parent.QuantizationScale.X / q);
            var y = (ushort)((vector2.Y - parent.QuantizationBias.Y) / parent.QuantizationScale.Y / q);

            return (x, y);
        }

        (ushort x, ushort y, ushort z) QuantizeVec3(Vector3 vector3)
        {
            var q = 1f / 65535f;

            var x = (ushort)((vector3.X - parent.BoxQuantizationBias.X) / parent.BoxQuantizationBias.X / q);
            var y = (ushort)((vector3.Y - parent.BoxQuantizationBias.Y) / parent.BoxQuantizationBias.Y / q);
            var z = (ushort)((vector3.Z - parent.BoxQuantizationBias.Z) / parent.BoxQuantizationBias.Z / q);

            return (x, y, z);
        }
    }
}