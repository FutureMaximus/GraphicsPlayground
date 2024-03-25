using GraphicsPlayground.Graphics.Terrain.Density;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

/// <summary>
/// Generates the mesh data for a chunk of terrain.
/// Based off of https://transvoxel.org/.
/// </summary>
public class TransvoxelMesher(in List<float> densityData, DensityGenerator densityGenerator)
{
    /// <summary> The data for the density of the terrain. </summary>
    public readonly List<float> DensityData = densityData;
    /// <summary> The generator used for the density data. </summary>
    public DensityGenerator DensityGenerator = densityGenerator;
    public Vector3i ChunkMin;
    public int LOD;
    public int NeighborsMask;
    /// <summary> The size of the chunk. </summary>
    public int ChunkSize;
    /// <summary> The cell width of the transition cells.</summary>
    public const float TRANSITION_CELL_WIDTH_PERCENTAGE = 0.5f;
    public TerrainMeshRenderContainer MeshDataContainer;

    /// <summary>Returns the density value at the given position.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetDensityValue(int x, int y, int z)
    {
        int densitySize = ChunkSize + 3;
        return DensityData[x * densitySize * densitySize + y * densitySize + z];
    }

    /// <summary>Returns the density value at the given position.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetDensityValue(Vector3i pos)
    {
        return GetDensityValue(pos.X, pos.Y, pos.Z);
    }

    #region Transvoxel Polygonize
    public void Polygonise()
    {
        int padding = 1;
        int lodScale = 1 << LOD;
        int[] currentCache = new int[ChunkSize * ChunkSize * 4];
        int[] previousCache = new int[ChunkSize * ChunkSize * 4];
        uint[] vertexIndices = new uint[16];
        float[] cellValues = new float[8];
        Vector3i paddingVec = new(padding);
        for (int y = 0; y < ChunkSize; y++)
        {
            for (int z = 0; z < ChunkSize; z++)
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    Vector3i cellPos = new(x, y, z);
                    for (int i = 0; i < 8; ++i)
                    {
                        Vector3i voxelPosition = cellPos + paddingVec + LengyelTables.RegularCornerOffset[i];
                        cellValues[i] = GetDensityValue(voxelPosition.X, voxelPosition.Y, voxelPosition.Z);
                    }
                    int caseCode = ((cellValues[0] < 0 ? 0x01 : 0)
                                  | (cellValues[1] < 0 ? 0x02 : 0)
                                  | (cellValues[2] < 0 ? 0x04 : 0)
                                  | (cellValues[3] < 0 ? 0x08 : 0)
                                  | (cellValues[4] < 0 ? 0x10 : 0)
                                  | (cellValues[5] < 0 ? 0x20 : 0)
                                  | (cellValues[6] < 0 ? 0x40 : 0)
                                  | (cellValues[7] < 0 ? 0x80 : 0));
                    currentCache[x * ChunkSize * 4 + z * 4] = -1;
                    if (caseCode == 0 || caseCode == 255)
                    {
                        continue;
                    }
                    int cacheValidator = ((cellPos.X != 0 ? 0x01 : 0)
                                        | (cellPos.Z != 0 ? 0x02 : 0)
                                        | (cellPos.Y != 0 ? 0x04 : 0));
                    int cellClass = LengyelTables.RegularCellClass[caseCode];
                    ushort[] edgeCodes = LengyelTables.RegularVertexData[caseCode];
                    LengyelTables.RegularCell regularCell = LengyelTables.RegularCellData[cellClass];
                    long cellVertCount = regularCell.GetVertexCount();
                    for (int i = 0; i < cellVertCount; ++i)
                    {
                        ushort edgeCode = edgeCodes[i];
                        ushort cornerIdx0 = (ushort)((edgeCode >> 4) & 0x0F);
                        ushort cornerIdx1 = (ushort)(edgeCode & 0x0F);
                        float density0 = cellValues[cornerIdx0];
                        float density1 = cellValues[cornerIdx1];
                        byte cacheIdx = (byte)((edgeCode >> 8) & 0x0F);
                        byte cacheDir = (byte)(edgeCode >> 12);
                        if (density1 == 0)
                        {
                            cacheDir = (byte)(cornerIdx1 ^ 7);
                            cacheIdx = 0;
                        }
                        else if (density0 == 0)
                        {
                            cacheDir = (byte)(cornerIdx0 ^ 7);
                            cacheIdx = 0;
                        }
                        bool vertexCacheable = (cacheDir & cacheValidator) == cacheDir;
                        int vertexIndex = -1;
                        int cachePosX = x - (cacheDir & 1);
                        int cachePosZ = z - ((cacheDir >> 1) & 1);
                        int[] selectedCacheDock = ((cacheDir >> 2) & 1) == 1 ? previousCache : currentCache;
                        if (vertexCacheable)
                        {
                            vertexIndex = selectedCacheDock[cachePosX * ChunkSize * 4 + cachePosZ * 4 + cacheIdx];
                        }
                        if (!vertexCacheable || vertexIndex == -1)
                        {
                            Vector3 vertex;
                            Vector3 normal;
                            vertexIndex = MeshDataContainer.MainData.Vertices.Count;
                            int vertBoundaryMask = 0;
                            if (cacheIdx == 0)
                            {
                                Vector3i cornerOffset = density0 == 0
                                    ? LengyelTables.RegularCornerOffset[cornerIdx0]
                                    : LengyelTables.RegularCornerOffset[cornerIdx1];
                                int vertPosX = x + cornerOffset.X;
                                int vertPosY = y + cornerOffset.Y;
                                int vertPosZ = z + cornerOffset.Z;
                                vertex = new Vector3i(vertPosX, vertPosY, vertPosZ);
                                if (LOD > 0)
                                {
                                    vertBoundaryMask = ((vertPosX == 0 ? 1 : 0)
                                                      | (vertPosY == 0 ? 2 : 0)
                                                      | (vertPosZ == 0 ? 4 : 0)
                                                      | (vertPosX == ChunkSize ? 8 : 0)
                                                      | (vertPosY == ChunkSize ? 16 : 0)
                                                      | (vertPosZ == ChunkSize ? 32 : 0));
                                }
                                vertPosX += padding;
                                vertPosY += padding;
                                vertPosZ += padding;
                                normal = new Vector3(GetDensityValue(vertPosX - 1, vertPosY, vertPosZ) - GetDensityValue(vertPosX + 1, vertPosY, vertPosZ),
                                                     GetDensityValue(vertPosX, vertPosY - 1, vertPosZ) - GetDensityValue(vertPosX, vertPosY + 1, vertPosZ),
                                                     GetDensityValue(vertPosX, vertPosY, vertPosZ - 1) - GetDensityValue(vertPosX, vertPosY, vertPosZ + 1));
                                if (vertexCacheable)
                                {
                                    selectedCacheDock[cachePosX * ChunkSize * 4 + cachePosZ * 4 + cacheIdx] = vertexIndex;
                                }
                            }
                            else
                            {
                                Vector3i vertLocalPos0 = cellPos + LengyelTables.RegularCornerOffset[cornerIdx0];
                                Vector3i vertLocalPos1 = cellPos + LengyelTables.RegularCornerOffset[cornerIdx1];
                                Vector3 vert0Copy = vertLocalPos0;
                                Vector3 vert1Copy = vertLocalPos1;
                                for (int j = 0; j < LOD; ++j)
                                {
                                    Vector3 midPointLocalPos = (vert0Copy + vert1Copy) * 0.5f;
                                    Vector3 midPointWorldPos = ChunkMin + midPointLocalPos * lodScale;
                                    float midPointDensity = DensityGenerator.GetValue(midPointWorldPos);
                                    if (Math.Sign(midPointDensity) == Math.Sign(density0))
                                    {
                                        vert0Copy = midPointLocalPos;
                                        density0 = midPointDensity;
                                    }
                                    else
                                    {
                                        vert1Copy = midPointLocalPos;
                                        density1 = midPointDensity;
                                    }
                                }
                                float t0 = density1 / (density1 - density0);
                                float t1 = 1 - t0;
                                vertex = vert0Copy * t0 + vert1Copy * t1;
                                int vertPosX0 = vertLocalPos0.X;
                                int vertPosY0 = vertLocalPos0.Y;
                                int vertPosZ0 = vertLocalPos0.Z;
                                int vertPosX1 = vertLocalPos1.X;
                                int vertPosY1 = vertLocalPos1.Y;
                                int vertPosZ1 = vertLocalPos1.Z;
                                if (LOD > 0)
                                {
                                    vertBoundaryMask = (((vertPosX0 == 0 || vertPosX1 == 0) ? 1 : 0)
                                                      | ((vertPosY0 == 0 || vertPosY1 == 0) ? 2 : 0)
                                                      | ((vertPosZ0 == 0 || vertPosZ1 == 0) ? 4 : 0)
                                                      | ((vertPosX0 == ChunkSize || vertPosX1 == ChunkSize) ? 8 : 0)
                                                      | ((vertPosY0 == ChunkSize || vertPosY1 == ChunkSize) ? 16 : 0)
                                                      | ((vertPosZ0 == ChunkSize || vertPosZ1 == ChunkSize) ? 32 : 0));
                                }
                                vertPosX0 += padding;
                                vertPosY0 += padding;
                                vertPosZ0 += padding;
                                vertPosX1 += padding;
                                vertPosY1 += padding;
                                vertPosZ1 += padding;
                                Vector3 normal0 = new(GetDensityValue(vertPosX0 - 1, vertPosY0, vertPosZ0) - GetDensityValue(vertPosX0 + 1, vertPosY0, vertPosZ0),
                                                            GetDensityValue(vertPosX0, vertPosY0 - 1, vertPosZ0) - GetDensityValue(vertPosX0, vertPosY0 + 1, vertPosZ0),
                                                            GetDensityValue(vertPosX0, vertPosY0, vertPosZ0 - 1) - GetDensityValue(vertPosX0, vertPosY0, vertPosZ0 + 1));
                                Vector3 normal1 = new(GetDensityValue(vertPosX1 - 1, vertPosY1, vertPosZ1) - GetDensityValue(vertPosX1 + 1, vertPosY1, vertPosZ1),
                                                            GetDensityValue(vertPosX1, vertPosY1 - 1, vertPosZ1) - GetDensityValue(vertPosX1, vertPosY1 + 1, vertPosZ1),
                                                            GetDensityValue(vertPosX1, vertPosY1, vertPosZ1 - 1) - GetDensityValue(vertPosX1, vertPosY1, vertPosZ1 + 1));
                                normal = normal0 + normal1;
                                if (cornerIdx1 == 7)
                                {
                                    currentCache[x * ChunkSize * 4 + z * 4 + cacheIdx] = vertexIndex;
                                }
                            }
                            normal.Normalize();
                            if (vertBoundaryMask > 0)
                            {
                                Vector3 Delta = new(0, 0, 0);
                                if ((vertBoundaryMask & 1) == 1 && vertex.X < 1)
                                {
                                    Delta.X = 1 - vertex.X;
                                }
                                else if ((vertBoundaryMask & 8) == 8 && vertex.X > (ChunkSize - 1))
                                {
                                    Delta.X = (ChunkSize - 1) - vertex.X;
                                }
                                if ((vertBoundaryMask & 2) == 2 && vertex.Y < 1)
                                {
                                    Delta.Y = 1 - vertex.Y;
                                }
                                else if ((vertBoundaryMask & 16) == 16 && vertex.Y > (ChunkSize - 1))
                                {
                                    Delta.Y = (ChunkSize - 1) - vertex.Y;
                                }
                                if ((vertBoundaryMask & 4) == 4 && vertex.Z < 1)
                                {
                                    Delta.Z = 1 - vertex.Z;
                                }
                                else if ((vertBoundaryMask & 32) == 32 && vertex.Z > (ChunkSize - 1))
                                {
                                    Delta.Z = (ChunkSize - 1) - vertex.Z;
                                }
                                Delta *= TRANSITION_CELL_WIDTH_PERCENTAGE;
                                Vector3 SecondaryVertPos = vertex + new Vector3(
                                            (1 - normal.X * normal.X) * Delta.X - normal.Y * normal.X * Delta.Y - normal.Z * normal.X * Delta.Z,
                                            -normal.X * normal.Y * Delta.X + (1 - normal.Y * normal.Y) * Delta.Y - normal.Z * normal.Y * Delta.Z,
                                            -normal.X * normal.Z * Delta.X - normal.Y * normal.Z * Delta.Y + (1 - normal.Z * normal.Z) * Delta.Z);
                                SecondaryVertexData SecondaryVertexData = new()
                                {
                                    Position = SecondaryVertPos * lodScale,
                                    VertexMask = (ushort)vertBoundaryMask,
                                    VertexIndex = (ushort)vertexIndex
                                };
                                MeshDataContainer.MainData.SecondaryVertices.Add(SecondaryVertexData);
                            }
                            MeshDataContainer.MainData.Vertices.Add(vertex * lodScale);
                            MeshDataContainer.MainData.Normals.Add(normal);
                        }
                        vertexIndices[i] = (uint)vertexIndex;
                    }
                    long indexCount = regularCell.GetTriangleCount() * 3;
                    byte[] cellIndices = regularCell.VertexIndex;
                    for (int i = 0; i < indexCount; i += 3)
                    {
                        uint ia = vertexIndices[cellIndices[i + 0]];
                        uint ic = vertexIndices[cellIndices[i + 2]];
                        uint ib = vertexIndices[cellIndices[i + 1]];

                        MeshDataContainer.MainData.Indices.Add(ia);
                        MeshDataContainer.MainData.Indices.Add(ib);
                        MeshDataContainer.MainData.Indices.Add(ic);
                    }
                }
            }
            (previousCache, currentCache) = (currentCache, previousCache);
        }
    }
    #endregion

    #region Transvoxel Transition Polygonize
    public void PolygoniseTransitions()
    {
        PolygoniseTransition(MeshDataContainer.LeftTransitionData, TransitionDirection.XMin);
        PolygoniseTransition(MeshDataContainer.DownTransitionData, TransitionDirection.YMin);
        PolygoniseTransition(MeshDataContainer.BackTransitionData, TransitionDirection.ZMin);
        PolygoniseTransition(MeshDataContainer.RightTransitionData, TransitionDirection.XMax);
        PolygoniseTransition(MeshDataContainer.UpTransitionData, TransitionDirection.YMax);
        PolygoniseTransition(MeshDataContainer.ForwardTransitionData, TransitionDirection.ZMax);
    }

    public void PolygoniseTransition(TerrainMeshRenderData renderData, TransitionDirection direction)
    {
        int padding = 1;
        int lodScale = 1 << LOD;
        int[] trCurrentCache = new int[ChunkSize * 10];
        int[] trPreviousCache = new int[ChunkSize * 10];
        uint[] trVertexIndices = new uint[36];
        float[] trCellValues = new float[13];
        Vector3i paddingVec = new(padding);
        for (int y = 0; y < ChunkSize; y++)
        {
            for (int x = 0; x < ChunkSize; x++)
            {
                Vector3i pos0 = FaceToLocalSpaceInt(direction, ChunkSize, x, y, 0) + paddingVec;
                Vector3i pos2 = FaceToLocalSpaceInt(direction, ChunkSize, x + 1, y, 0) + paddingVec;
                Vector3i pos6 = FaceToLocalSpaceInt(direction, ChunkSize, x, y + 1, 0) + paddingVec;
                Vector3i pos8 = FaceToLocalSpaceInt(direction, ChunkSize, x + 1, y + 1, 0) + paddingVec;
                Vector3 pos1 = ChunkMin + FaceToLocalSpaceFloat(direction, ChunkSize, x + 0.5f, y, 0) * lodScale;
                Vector3 pos3 = ChunkMin + FaceToLocalSpaceFloat(direction, ChunkSize, x, y + 0.5f, 0) * lodScale;
                Vector3 pos4 = ChunkMin + FaceToLocalSpaceFloat(direction, ChunkSize, x + 0.5f, y + 0.5f, 0) * lodScale;
                Vector3 pos5 = ChunkMin + FaceToLocalSpaceFloat(direction, ChunkSize, x + 1.0f, y + 0.5f, 0) * lodScale;
                Vector3 pos7 = ChunkMin + FaceToLocalSpaceFloat(direction, ChunkSize, x + 0.5f, y + 1.0f, 0) * lodScale;
                trCellValues[0] = GetDensityValue(pos0);
                trCellValues[2] = GetDensityValue(pos2);
                trCellValues[6] = GetDensityValue(pos6);
                trCellValues[8] = GetDensityValue(pos8);
                trCellValues[1] = DensityGenerator.GetValue(pos1);
                trCellValues[3] = DensityGenerator.GetValue(pos3);
                trCellValues[4] = DensityGenerator.GetValue(pos4);
                trCellValues[5] = DensityGenerator.GetValue(pos5);
                trCellValues[7] = DensityGenerator.GetValue(pos7);
                trCellValues[0x9] = trCellValues[0];
                trCellValues[0xA] = trCellValues[2];
                trCellValues[0xB] = trCellValues[6];
                trCellValues[0xC] = trCellValues[8];
                int caseCode = ((trCellValues[0] < 0 ? 1 : 0)
                              | (trCellValues[1] < 0 ? 2 : 0)
                              | (trCellValues[2] < 0 ? 4 : 0)
                              | (trCellValues[5] < 0 ? 8 : 0)
                              | (trCellValues[8] < 0 ? 16 : 0)
                              | (trCellValues[7] < 0 ? 32 : 0)
                              | (trCellValues[6] < 0 ? 64 : 0)
                              | (trCellValues[3] < 0 ? 128 : 0)
                              | (trCellValues[4] < 0 ? 256 : 0));
                trCurrentCache[0 * ChunkSize + x] = -1;
                trCurrentCache[1 * ChunkSize + x] = -1;
                trCurrentCache[2 * ChunkSize + x] = -1;
                trCurrentCache[7 * ChunkSize + x] = -1;
                if (caseCode == 0 || caseCode == 511)
                {
                    continue;
                }
                int cacheValidator = ((x != 0 ? 0b01 : 0) | (y != 0 ? 0b10 : 0));
                int cellClass = LengyelTables.TransitionCellClass[caseCode];
                ushort[] edgeCodes = LengyelTables.TransitionVertexData[caseCode];
                LengyelTables.RegularCell cellData = LengyelTables.TransitionRegularCellData[cellClass & 0x7F];
                long cellVertCount = cellData.GetVertexCount();
                for (int i = 0; i < cellVertCount; ++i)
                {
                    ushort edgeCode = edgeCodes[i];
                    ushort cornerIdx0 = (ushort)((edgeCode >> 4) & 0x0F);
                    ushort cornerIdx1 = (ushort)(edgeCode & 0x0F);
                    float density0 = trCellValues[cornerIdx0];
                    float density1 = trCellValues[cornerIdx1];
                    byte cacheIdx = (byte)((edgeCode >> 8) & 0x0F);
                    byte cacheDir = (byte)(edgeCode >> 12);
                    if (density1 == 0)
                    {
                        byte trCornerData = LengyelTables.TransitionCornerData[cornerIdx1];
                        cacheDir = (byte)((trCornerData >> 4) & 0x0F);
                        cacheIdx = (byte)(trCornerData & 0x0F);
                    }
                    else if (density0 == 0)
                    {
                        byte trCornerData = LengyelTables.TransitionCornerData[cornerIdx0];
                        cacheDir = (byte)((trCornerData >> 4) & 0x0F);
                        cacheIdx = (byte)(trCornerData & 0x0F);
                    }
                    bool vertexCacheable = (cacheDir & cacheValidator) == cacheDir;
                    int vertexIndex = -1;
                    int cachePosX = x - (cacheDir & 1);
                    int[] selectedCacheDock = (cacheDir & 2) > 0 ? trPreviousCache : trCurrentCache;
                    if (vertexCacheable)
                    {
                        vertexIndex = selectedCacheDock[cacheIdx * ChunkSize + cachePosX];
                    }
                    if (!vertexCacheable || vertexIndex == -1)
                    {
                        Vector3 vertex;
                        Vector3 normal;
                        vertexIndex = renderData.Vertices.Count;
                        int vertBoundaryMask = 0;
                        bool isLowResFace = cacheIdx > 6;
                        if (density0 == 0 || density1 == 0)
                        {
                            int cornerIdx = density0 == 0 ? cornerIdx0 : cornerIdx1;
                            Vector3i cornerOffset = LengyelTables.TransitionCornerOffset[cornerIdx];
                            vertex = FaceToLocalSpaceFloat(direction, ChunkSize, x + cornerOffset.X * 0.5f, y + cornerOffset.Y * 0.5f, 0);
                            if (isLowResFace)
                            {
                                Vector3i vertLocalPos = FaceToLocalSpaceInt(direction, ChunkSize, x + cornerOffset.X / 2, y + cornerOffset.Y / 2, 0);
                                int locX = vertLocalPos.X;
                                int locY = vertLocalPos.Y;
                                int locZ = vertLocalPos.Z;
                                vertBoundaryMask = ((locX == 0 ? 1 : 0)
                                                  | (locY == 0 ? 2 : 0)
                                                  | (locZ == 0 ? 4 : 0)
                                                  | (locX == ChunkSize ? 8 : 0)
                                                  | (locY == ChunkSize ? 16 : 0)
                                                  | (locZ == ChunkSize ? 32 : 0));
                                locX += padding;
                                locY += padding;
                                locZ += padding;
                                normal = new Vector3(GetDensityValue(locX - 1, locY, locZ) - GetDensityValue(locX + 1, locY, locZ),
                                                    GetDensityValue(locX, locY - 1, locZ) - GetDensityValue(locX, locY + 1, locZ),
                                                    GetDensityValue(locX, locY, locZ - 1) - GetDensityValue(locX, locY, locZ + 1));
                            }
                            else
                            {
                                float wX = ChunkMin.X + vertex.X * lodScale;
                                float wY = ChunkMin.Y + vertex.Y * lodScale;
                                float wZ = ChunkMin.Z + vertex.Z * lodScale;
                                normal = new Vector3(DensityGenerator.GetValue(new Vector3(wX - 1, wY, wZ)) - DensityGenerator.GetValue(new Vector3(wX + 1, wY, wZ)),
                                                    DensityGenerator.GetValue(new Vector3(wX, wY - 1, wZ)) - DensityGenerator.GetValue(new Vector3(wX, wY + 1, wZ)),
                                                    DensityGenerator.GetValue(new Vector3(wX, wY, wZ - 1)) - DensityGenerator.GetValue(new Vector3(wX, wY, wZ + 1)));
                            }
                            if (cacheDir == 8)
                            {
                                trCurrentCache[cacheIdx * ChunkSize + x] = vertexIndex;
                            }
                            else if (vertexCacheable)
                            {
                                selectedCacheDock[cacheIdx * ChunkSize + cachePosX] = vertexIndex;
                            }
                        }
                        else
                        {
                            Vector3i cornerOffset0 = LengyelTables.TransitionCornerOffset[cornerIdx0];
                            Vector3i cornerOffset1 = LengyelTables.TransitionCornerOffset[cornerIdx1];
                            Vector3 corner0Copy = FaceToLocalSpaceFloat(direction, ChunkSize, x + cornerOffset0.X * 0.5f, y + cornerOffset0.Y * 0.5f, 0);
                            Vector3 corner1Copy = FaceToLocalSpaceFloat(direction, ChunkSize, x + cornerOffset1.X * 0.5f, y + cornerOffset1.Y * 0.5f, 0);
                            int subEdges = isLowResFace ? LOD : LOD - 1;
                            for (int j = 0; j < subEdges; ++j)
                            {
                                Vector3 midPointLocalPos = (corner0Copy + corner1Copy) * 0.5f;
                                Vector3 midPointWorldPos = ChunkMin + midPointLocalPos * lodScale;
                                float midPointDensity = DensityGenerator.GetValue(midPointWorldPos);
                                if (Math.Sign(midPointDensity) == Math.Sign(density0))
                                {
                                    corner0Copy = midPointLocalPos;
                                    density0 = midPointDensity;
                                }
                                else
                                {
                                    corner1Copy = midPointLocalPos;
                                    density1 = midPointDensity;
                                }
                            }
                            float t0 = density1 / (density1 - density0);
                            float t1 = 1 - t0;
                            vertex = corner0Copy * t0 + corner1Copy * t1;
                            Vector3 normal0, normal1;
                            if (isLowResFace)
                            {
                                Vector3i vert0LocPos = FaceToLocalSpaceInt(direction, ChunkSize, x + cornerOffset0.X / 2, y + cornerOffset0.Y / 2, 0);
                                Vector3i vert1LocPos = FaceToLocalSpaceInt(direction, ChunkSize, x + cornerOffset1.X / 2, y + cornerOffset1.Y / 2, 0);
                                int VX0 = vert0LocPos.X;
                                int VY0 = vert0LocPos.Y;
                                int VZ0 = vert0LocPos.Z;
                                int VX1 = vert1LocPos.X;
                                int VY1 = vert1LocPos.Y;
                                int VZ1 = vert1LocPos.Z;
                                vertBoundaryMask = ((VX0 == 0 || VX1 == 0 ? 1 : 0)
                                                  | (VY0 == 0 || VY1 == 0 ? 2 : 0)
                                                  | (VZ0 == 0 || VZ1 == 0 ? 4 : 0)
                                                  | (VX0 == ChunkSize || VX1 == ChunkSize ? 8 : 0)
                                                  | (VY0 == ChunkSize || VY1 == ChunkSize ? 16 : 0)
                                                  | (VZ0 == ChunkSize || VZ1 == ChunkSize ? 32 : 0));
                                VX0 += padding;
                                VY0 += padding;
                                VZ0 += padding;
                                VX1 += padding;
                                VY1 += padding;
                                VZ1 += padding;
                                normal0 = new Vector3(GetDensityValue(VX0 - 1, VY0, VZ0) - GetDensityValue(VX0 + 1, VY0, VZ0),
                                                     GetDensityValue(VX0, VY0 - 1, VZ0) - GetDensityValue(VX0, VY0 + 1, VZ0),
                                                     GetDensityValue(VX0, VY0, VZ0 - 1) - GetDensityValue(VX0, VY0, VZ0 + 1));
                                normal1 = new Vector3(GetDensityValue(VX1 - 1, VY1, VZ1) - GetDensityValue(VX1 + 1, VY1, VZ1),
                                                     GetDensityValue(VX1, VY1 - 1, VZ1) - GetDensityValue(VX1, VY1 + 1, VZ1),
                                                     GetDensityValue(VX1, VY1, VZ1 - 1) - GetDensityValue(VX1, VY1, VZ1 + 1));
                            }
                            else
                            {
                                Vector3 vert0LocPos = FaceToLocalSpaceFloat(direction, ChunkSize, x + cornerOffset0.X * 0.5f, y + cornerOffset0.Y * 0.5f, 0);
                                Vector3 vert1LocPos = FaceToLocalSpaceFloat(direction, ChunkSize, x + cornerOffset1.X * 0.5f, y + cornerOffset1.Y * 0.5f, 0);
                                float wX0 = ChunkMin.X + vert0LocPos.X * lodScale;
                                float wY0 = ChunkMin.Y + vert0LocPos.Y * lodScale;
                                float wZ0 = ChunkMin.Z + vert0LocPos.Z * lodScale;
                                float wX1 = ChunkMin.X + vert1LocPos.X * lodScale;
                                float wY1 = ChunkMin.Y + vert1LocPos.Y * lodScale;
                                float wZ1 = ChunkMin.Z + vert1LocPos.Z * lodScale;
                                normal0 = new Vector3(DensityGenerator.GetValue(new Vector3(wX0 - 1, wY0, wZ0)) - DensityGenerator.GetValue(new Vector3(wX0 + 1, wY0, wZ0)),
                                                     DensityGenerator.GetValue(new Vector3(wX0, wY0 - 1, wZ0)) - DensityGenerator.GetValue(new Vector3(wX0, wY0 + 1, wZ0)),
                                                     DensityGenerator.GetValue(new Vector3(wX0, wY0, wZ0 - 1)) - DensityGenerator.GetValue(new Vector3(wX0, wY0, wZ0 + 1)));
                                normal1 = new Vector3(DensityGenerator.GetValue(new Vector3(wX1 - 1, wY1, wZ1)) - DensityGenerator.GetValue(new Vector3(wX1 + 1, wY1, wZ1)),
                                                     DensityGenerator.GetValue(new Vector3(wX1, wY1 - 1, wZ1)) - DensityGenerator.GetValue(new Vector3(wX1, wY1 + 1, wZ1)),
                                                     DensityGenerator.GetValue(new Vector3(wX1, wY1, wZ1 - 1)) - DensityGenerator.GetValue(new Vector3(wX1, wY1, wZ1 + 1)));
                            }
                            normal = normal0 + normal1;
                            if (cacheDir == 8)
                            {
                                trCurrentCache[cacheIdx * ChunkSize + x] = vertexIndex;
                            }
                            else if (vertexCacheable && cacheDir != 4)
                            {
                                selectedCacheDock[cacheIdx * ChunkSize + cachePosX] = vertexIndex;
                            }
                        }
                        normal.Normalize();
                        if (vertBoundaryMask > 0)
                        {
                            Vector3 delta = Vector3.Zero;
                            if ((vertBoundaryMask & 1) == 1 && vertex.X < 1)
                            {
                                delta.X = 1 - vertex.X;
                            }
                            else if ((vertBoundaryMask & 8) == 8 && vertex.X > (ChunkSize - 1))
                            {
                                delta.X = (ChunkSize - 1) - vertex.X;
                            }
                            if ((vertBoundaryMask & 2) == 2 && vertex.Y < 1)
                            {
                                delta.Y = 1 - vertex.Y;
                            }
                            else if ((vertBoundaryMask & 16) == 16 && vertex.Y > (ChunkSize - 1))
                            {
                                delta.Y = (ChunkSize - 1) - vertex.Y;
                            }
                            if ((vertBoundaryMask & 4) == 4 && vertex.Z < 1)
                            {
                                delta.Z = 1 - vertex.Z;
                            }
                            else if ((vertBoundaryMask & 32) == 32 && vertex.Z > (ChunkSize - 1))
                            {
                                delta.Z = (ChunkSize - 1) - vertex.Z;
                            }
                            delta *= TRANSITION_CELL_WIDTH_PERCENTAGE;
                            Vector3 vertexSecondaryPos = vertex + new Vector3(
                                    (1 - normal.X * normal.X) * delta.X - normal.Y * normal.X * delta.Y - normal.Z * normal.X * delta.Z,
                                       -normal.X * normal.Y * delta.X + (1 - normal.Y * normal.Y) * delta.Y - normal.Z * normal.Y * delta.Z,
                                       -normal.X * normal.Z * delta.X - normal.Y * normal.Z * delta.Y + (1 - normal.Z * normal.Z) * delta.Z);
                            renderData.SecondaryVertices.Add(new SecondaryVertexData()
                            {
                                Position = vertexSecondaryPos * lodScale,
                                VertexMask = (ushort)vertBoundaryMask,
                                VertexIndex = (ushort)vertexIndex
                            });
                        }
                        renderData.Vertices.Add(vertex * lodScale);
                        renderData.Normals.Add(normal);
                    }
                    trVertexIndices[i] = (uint)vertexIndex;
                }
                long indexCount = cellData.GetTriangleCount() * 3;
                byte[] cellIndices = cellData.VertexIndex;
                bool flipWinding = (cellClass & 0x80) > 0;
                for (int i = 0; i < indexCount; i += 3)
                {
                    uint ia = trVertexIndices[cellIndices[i + 0]];
                    uint ib = trVertexIndices[cellIndices[i + 1]];
                    uint ic = trVertexIndices[cellIndices[i + 2]];
                    if (!flipWinding)
                    {
                        renderData.Indices.Add(ic);
                        renderData.Indices.Add(ib);
                        renderData.Indices.Add(ia);
                    }
                    else
                    {
                        renderData.Indices.Add(ia);
                        renderData.Indices.Add(ib);
                        renderData.Indices.Add(ic);
                    }
                }
            }
            (trPreviousCache, trCurrentCache) = (trCurrentCache, trPreviousCache);
        }
    }
    #endregion

    #region Polygonize Helpers
    public enum TransitionDirection
    {
        XMin,
        YMin,
        ZMin,
        XMax,
        YMax,
        ZMax,
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i FaceToLocalSpaceInt(TransitionDirection transitionDir, int chunkSize, int x, int y, int z)
    {
        return transitionDir switch
        {
            TransitionDirection.XMin => new Vector3i(z, x, y),
            TransitionDirection.XMax => new Vector3i(chunkSize - z, y, x),
            TransitionDirection.YMin => new Vector3i(y, z, x),
            TransitionDirection.YMax => new Vector3i(x, chunkSize - z, y),
            TransitionDirection.ZMin => new Vector3i(x, y, z),
            TransitionDirection.ZMax => new Vector3i(y, x, chunkSize - z),
            _ => new Vector3i(x, y, z),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 FaceToLocalSpaceFloat(TransitionDirection transitionDir, int chunkSize, float x, float y, float z)
    {
        return transitionDir switch
        {
            TransitionDirection.XMin => new Vector3(z, x, y),
            TransitionDirection.XMax => new Vector3(chunkSize - z, y, x),
            TransitionDirection.YMin => new Vector3(y, z, x),
            TransitionDirection.YMax => new Vector3(x, chunkSize - z, y),
            TransitionDirection.ZMin => new Vector3(x, y, z),
            TransitionDirection.ZMax => new Vector3(y, x, chunkSize - z),
            _ => new Vector3(x, y, z),
        };
    }
    #endregion
}
