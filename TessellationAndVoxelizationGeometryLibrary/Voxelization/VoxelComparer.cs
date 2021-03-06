﻿// ***********************************************************************
// Assembly         : TessellationAndVoxelizationGeometryLibrary
// Author           : Design Engineering Lab
// Created          : 02-27-2015
//
// Last Modified By : Matt Campbell
// Last Modified On : 09-21-2017
// ***********************************************************************
// <copyright file="VoxelComparer.cs" company="Design Engineering Lab">
//     Copyright ©  2017
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;

namespace TVGL.Voxelization
{
    public abstract class VoxelComparer : IEqualityComparer<long>
    {
        protected List<long> parentMasks;
        protected int coordShift, yShift, zShift;
        protected long coordMask, mask;
        internal long EqualsMask(long id)
        {
            return id & mask;
        }
        internal long ParentMask(long id, int level)
        {
            return id & parentMasks[level];
        }

        internal bool IsDescendantOf(long entry, long ancestor, int level)
        {
            return ParentMask(entry, level) == ancestor;
        }
        public bool Equals(long entry, long query)
        {
            return EqualsMask(entry) == query;
        }

        public int GetHashCode(long id)
        {
            var x = (id >> coordShift + 4) & coordMask;
            var y = (id >> coordShift + 24) & coordMask;
            var z = (id >> coordShift + 44) & coordMask;
            return (int)(x + (y << yShift) + (z << zShift));
        }
        internal void MakeParentMasks(int[] bitLevelDistribution, int numberOfBitsInParent)
        {
            parentMasks = new List<long>();
            var k = 0;
            var numBits = 0;
            do
            {
                numBits += bitLevelDistribution[k++];
                var bitMask = (long)Math.Pow(2, numBits) - 1;
                var parentShift = 20 - numBits;
                parentMasks.Add((bitMask << parentShift + 4) + (bitMask << parentShift + 24) +
                                (bitMask << parentShift + 44));
            } while (numBits < numberOfBitsInParent);
        }
    }


    public class VoxelComparerLevel0 : VoxelComparer
    {
        internal VoxelComparerLevel0(int bitsInLevel0)
        {
            // level0 is at the far left of the coordinate amount and since
            // each coordinate takes up to 20 bits, we subtract
            coordShift = 20 - bitsInLevel0;
            // yShift and zShift are just left-shifted to follow x
            yShift = bitsInLevel0;
            zShift = 2 * bitsInLevel0;
            // the coordinate mask should be all one's for 3 bits its 111, for 4 it's 1111,
            // etc. This formula creates that number. For 5 bits, its 2^5-1 = 31
            coordMask = (int)Math.Pow(2, bitsInLevel0) - 1;
            // mask is usually just the coordinates without the flags. however, since
            // many queries are simply lower level ID's - this allows us to check just the
            // bits at level 0
            mask = (coordMask << coordShift + 4) + (coordMask << coordShift + 24)
                                               + (coordMask << coordShift + 44);
        }
    }

    public class VoxelComparerMidLevels : VoxelComparer
    {
        internal VoxelComparerMidLevels(int[] bitLevelDistribution, int numberOfBitsInParent)
        {
            MakeParentMasks(bitLevelDistribution, numberOfBitsInParent);
            // the midlevel bits are the 10 bits following the level0 bits.
            // why 10? because for x, y, and z this would be 30 bits and the
            // hashsets define the hash code as a 31-bit (positive integer)
            // the bits from level0 do not need to be included as there are hashsets
            // within each level0 voxel
            coordShift = 20 - 10 - bitLevelDistribution[0];
            // yShift and zShift are just left-shifted to follow x
            yShift = 10;
            zShift = 20;
            coordMask = 1023;
            mask = (coordMask << coordShift + 4) + (coordMask << coordShift + 24)
                                                 + (coordMask << coordShift + 44);
        }
    }
    /// <summary>
    /// Class VoxelComparerFine.
    /// </summary>
    public class VoxelComparerFine : VoxelComparer
    {
        internal VoxelComparerFine(int[] bitLevelDistribution, int numberOfBitsInParent)
        {
            MakeParentMasks(bitLevelDistribution, numberOfBitsInParent);
            // assuming that we are using all the bits, so no need to shift each coordinate
            coordShift = 0;
            // here, there will be overlap in the hashset, to make it
            var bitsInCoord = 20 - bitLevelDistribution[0];
            zShift = 31 - bitsInCoord;
            yShift = zShift / 2;
            coordMask = (int)Math.Pow(2, bitsInCoord) - 1;
            mask = (coordMask << coordShift + 4) + (coordMask << coordShift + 24)
                                                 + (coordMask << coordShift + 44);
        }

    }
    public class VoxelToTessellationComparer : IEqualityComparer<long>
    {
        int coordShift, yShift, zShift;
        long equalsMask, coordMask;

        internal VoxelToTessellationComparer(int numberOfBitsInLevel0)
        {
            coordShift = 20 - 10 - numberOfBitsInLevel0;
            // yShift and zShift are just left-shifted to follow x
            yShift = 10;
            zShift = 20;
            coordMask = 1023;
            var maskOfAllLevels = (long)Math.Pow(2, numberOfBitsInLevel0 + 10) - 1;
            equalsMask = (maskOfAllLevels << coordShift + 4) + (maskOfAllLevels << coordShift + 24)
                                                 + (maskOfAllLevels << coordShift + 44);
        }

        public bool Equals(long x, long y)
        {
            return (x & equalsMask) == (y & equalsMask);

        }

        public int GetHashCode(long id)
        {
            var x = (id >> coordShift + 4) & coordMask;
            var y = (id >> coordShift + 24) & coordMask;
            var z = (id >> coordShift + 44) & coordMask;
            return (int)(x + (y << yShift) + (z << zShift));
        }
    }

}