using WolvenKit.Core.Extensions;
using WolvenKit.RED4.Archive.Buffer;
using WolvenKit.RED4.IO;
using WolvenKit.RED4.Types;

namespace WolvenKit.RED4.Archive.IO;

public class MinimapEncodedShapesReader : Red4Reader, IBufferReader
{
    public MinimapEncodedShapesReader(MemoryStream ms) : base(ms)
    {

    }

    public EFileReadErrorCodes ReadBuffer(RedBuffer buffer)
    {
        if (buffer.Parent is not minimapEncodedShapes minimapEncodedShapes)
        {
            throw new Exception();
        }

        var data = new MinimapEncodedShapesBuffer();

        for (var i = 0; i < minimapEncodedShapes.NumShapes; i++)
        {
            data.ShapeAddresses.Add(new MinimapEncodedShapesBuffer_ShapeAddress
            {
                VertexIndex = ReadCUInt32(),
                Count = ReadCUInt16(),
                BoundsIndex = ReadCUInt16()
            });
        }

        for (var i = 0; i < minimapEncodedShapes.NumShapes; i++)
        {
            data.EncodedShapeInfos.Add(new MinimapEncodedShapesBuffer_EncodedShapeInfo
            {
                OwnerIndex = ReadCUInt16(),
                ShapeType = ReadCUInt8(),
                TypeFlag = ReadCUInt8()
            });
        }

        for (var i = 0; i < minimapEncodedShapes.NumShapes; i++)
        {
            data.LocalsToModels.Add(new Vector3
            {
                X = ReadCFloat(),
                Y = ReadCFloat(),
                Z = ReadCFloat()
            });
        }

        for (var i = 0; i < minimapEncodedShapes.NumSpatialBuckets; i++)
        {
            data.Buckets.Add(new MinimapEncodedShapesBuffer_Bucket
            {
                Begin = ReadCUInt16(),
                Count = ReadCUInt16()
            });
        }

        for (var i = 0; i < minimapEncodedShapes.NumSpatialBuckets; i++)
        {
            data.BucketsBounds.Add(new Box
            {
                Min = new Vector4 { X = ReadCFloat(), Y = ReadCFloat(), Z = ReadCFloat(), W = ReadCFloat() },
                Max = new Vector4 { X = ReadCFloat(), Y = ReadCFloat(), Z = ReadCFloat(), W = ReadCFloat() }
            });
        }

        for (var i = 0; i < minimapEncodedShapes.NumShapes; i++)
        {
            /*var val = _reader.ReadUInt64();

            data.Indices.Add(new MinimapEncodedShapesBuffer_ShapeIndex
            {
                PaddingUnk =       (CUInt8)  ((val >> (64 - 12)) & 0xFF),
                NumFillIndices =   (CUInt16) ((val >> (64 - 12 - 12)) & 0xFFF),
                NumBorderIndices = (CUInt16) ((val >> (64 - 12 - 12 - 16)) & 0xFFFF),
                FillIndex =        (CUInt16) ((val >> (64 - 12 - 12 - 16 - 16)) & 0xFFFF),
                BorderIndex =      (CUInt16) ((val >> (64 - 12 - 12 - 16 - 16 - 8)) & 0xFFF),
            });*/

            var val = _reader.BaseStream.ReadBitfield<MinimapEncodedShapesBuffer_ShapeIndexStruct>();
            data.Indices.Add(MinimapEncodedShapesBuffer_ShapeIndex.FromStruct(val));
        }

        for (var i = 0; i < minimapEncodedShapes.NumUniqueGeometry; i++)
        {
            data.LocalBounds.Add(new MinimapEncodedShapesBuffer_Box
            {
                Min = DeQuantizeVec3(_reader.ReadUInt16(), _reader.ReadUInt16(), _reader.ReadUInt16()),
                Max = DeQuantizeVec3(_reader.ReadUInt16(), _reader.ReadUInt16(), _reader.ReadUInt16())
            });
        }

        for (var i = 0; i < minimapEncodedShapes.NumBorderPoints; i++)
        {
            data.Vertices.Add(DeQuantizeVec2(_reader.ReadUInt16(), _reader.ReadUInt16()));
        }

        for (var i = 0; i < minimapEncodedShapes.NumFillPoints; i++)
        {
            data.VertexIndices.Add(ReadCUInt16());
        }

        for (var i = 0; i < minimapEncodedShapes.NumOwners; i++)
        {
            data.Owners.Add(ReadCUInt64());
        }

        buffer.Data = data;

        return EFileReadErrorCodes.NoError;

        Vector2 DeQuantizeVec2(ushort x, ushort y)
        {
            var q = 1f / 65535f;

            return new Vector2()
            {
                X = (x * q * minimapEncodedShapes.QuantizationScale.X) + minimapEncodedShapes.QuantizationBias.X,
                Y = (y * q * minimapEncodedShapes.QuantizationScale.Y) + minimapEncodedShapes.QuantizationBias.Y
            };
        }

        Vector3 DeQuantizeVec3(ushort x, ushort y, ushort z)
        {
            var q = 1f / 65535f;

            return new Vector3()
            {
                X = (x * q * minimapEncodedShapes.BoxQuantizationScale.X) + minimapEncodedShapes.BoxQuantizationBias.X,
                Y = (y * q * minimapEncodedShapes.BoxQuantizationScale.Y) + minimapEncodedShapes.BoxQuantizationBias.Y,
                Z = (z * q * minimapEncodedShapes.BoxQuantizationScale.Z) + minimapEncodedShapes.BoxQuantizationBias.Z,
            };
        }
    }
}