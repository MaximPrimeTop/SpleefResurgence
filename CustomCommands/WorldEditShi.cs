using Terraria;

namespace SpleefResurgence.CustomCommands
{
    public class WorldEdit
    {
        public static void Rise(int x1, int y1, int x2, int y2, byte liquidType)
        {
            int left = Math.Min(x1, x2), right = Math.Max(x1, x2);
            int top = Math.Min(y1, y2), bottom = Math.Max(y1, y2);

            for (int j = left; j <= right; j++)
            {
                for (int k = top; k <= bottom; k++)
                {
                    Main.tile[j, k].ClearTile();
                    NetMessage.SendTileSquare(-1, j, k, 1);
                    WorldGen.PlaceLiquid(j, k, liquidType, 255);
                }
            }
            Liquid.UpdateLiquid();
        }

        public static void PaintBlock(int x, int y, byte paint)
        {
            WorldGen.paintTile(x, y, paint, true);
            NetMessage.SendTileSquare(-1, x, y, 1);
        }

        public static void ReplaceBlock(int x, int y, ushort targetBlock, ushort replaceBlock)
        {
            if (Main.tile[x, y].active() && Main.tile[x, y].type == targetBlock)
            {
                WorldGen.PlaceTile(x, y, replaceBlock, true, true);
                NetMessage.SendTileSquare(-1, x, y, 1);
            }
        }

        public static void PaintArea(int x1, int y1, int x2, int y2, byte paint)
        {
            int left = Math.Min(x1, x2), right = Math.Max(x1, x2);
            int top = Math.Min(y1, y2), bottom = Math.Max(y1, y2);

            for (int j = left; j <= right; j++)
                for (int k = top; k <= bottom; k++)
                    PaintBlock(j, k, paint);
        }

        public static void ReplaceArea(int x1, int y1, int x2, int y2, ushort targetBlock, ushort replaceBlock)
        {
            int left = Math.Min(x1, x2), right = Math.Max(x1, x2);
            int top = Math.Min(y1, y2), bottom = Math.Max(y1, y2);

            for (int j = left; j <= right; j++)
                for (int k = top; k <= bottom; k++)
                    ReplaceBlock(j, k, targetBlock, replaceBlock);
        }
    }
}
