namespace KianCommons.Math {
    using System.Collections.Generic;
    using UnityEngine;

    public struct Grid {
        public float CellSize;
        public int Resolution;
        public Grid(float cellSize, int resolution) {
            CellSize = cellSize;
            Resolution = resolution;
        }

        public int ConvertGrid(float a) =>
            Mathf.Clamp((int)(a / CellSize + (Resolution / 2)), 0, Resolution - 1);
        public float ConvertStartPoint(int a) =>
            (a - Resolution / 2f) * CellSize;
    }

    public struct IntVector2 {
        public int x;
        public int y;

        public IntVector2(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public static IntVector2 up = new IntVector2(0, 1);
        public static IntVector2 down = new IntVector2(0, -1);
        public static IntVector2 left = new IntVector2(-1, 0);
        public static IntVector2 right = new IntVector2(1, 0);
        public static IntVector2 zero = new IntVector2(0, 0);

        public int MangitudeSquare => x * x + y * y;
    }

    /// <summary>
    /// generic version of GridVector
    /// </summary>
    public struct GridVector2 {
        public IntVector2 Vector;
        public Grid Grid;

        public GridVector2(float x, float z, Grid grid) {
            Grid = grid;
            Vector.x = grid.ConvertGrid(x);
            Vector.y = grid.ConvertGrid(z);
        }

        public GridVector2(Vector2 v, Grid grid) : this(v.x, v.y, grid) { }
        public GridVector2(Vector3 v, Grid grid) : this(v.x, v.z, grid) { }

        public int X => Vector.x;
        public int Z => Vector.y;
        public int Index => Z * Grid.Resolution + X;
        public int MangitudeSquare => Vector.MangitudeSquare;

        public static GridVector2 operator +(GridVector2 lhs, GridVector2 rhs) =>
            new GridVector2(lhs.X + rhs.X, lhs.X + rhs.Z, lhs.Grid);

        public static GridVector2 operator -(GridVector2 lhs, GridVector2 rhs) =>
            new GridVector2(lhs.X - rhs.X, lhs.Z - rhs.Z, lhs.Grid);

        public static GridVector2 operator +(GridVector2 lhs, IntVector2 rhs) =>
            new GridVector2(lhs.X + rhs.x, lhs.Z + rhs.y, lhs.Grid);

        public static GridVector2 operator -(GridVector2 lhs, IntVector2 rhs) =>
            new GridVector2(lhs.X - rhs.x, lhs.Z - rhs.y, lhs.Grid);

        public override string ToString() {
            return $"GridVector<{Index}>({X}, {Z} | cellsize={Grid.CellSize}, resolution={Grid.Resolution})";
        }

        public IEnumerable<GridVector2> ScanArea(int size = 1) {
            for (int deltax = -size; deltax <= size; ++deltax) {
                for (int deltaz = -size; deltaz <= size; ++deltaz) {
                    IntVector2 delta = new IntVector2(deltax, deltaz);
                    var ret = this + delta;
                    if(ret.Index>=0 && ret.Index < Grid.Resolution) {
                        yield return ret;
                    }
                }
            }
        }
    }
}
