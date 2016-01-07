﻿using System;
using System.Collections.Generic;
using Papyrus.Business.VersionRanges;

namespace Papyrus.Business.Topics.Exceptions
{
    public class VersionRangesCollisionException : Exception
    {
        public VersionRangesCollisionException(List<Collision> conflictedRanges) : base(ConstructMessageFrom(conflictedRanges))
        {
        }

        private static string ConstructMessageFrom(List<Collision> conflictedRanges)
        {
            var rangesToShow = "";
            foreach (var editableVersionRange in conflictedRanges)
            {
                rangesToShow += ParseToString(editableVersionRange.FirstVersionRange) + " collide with "
                                + ParseToString(editableVersionRange.SecondVersionRange) + "\n";
            }
            return rangesToShow;
        }

        private static string ParseToString(EditableVersionRange editableVersionRange)
        {
            return "(" + editableVersionRange.FromVersion.VersionName + " -- " +
                   editableVersionRange.ToVersion.VersionName + ")";
        }
    }
}