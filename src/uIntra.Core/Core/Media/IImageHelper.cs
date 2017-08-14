﻿using System.IO;

namespace uIntra.Core.Media
{
    public interface IImageHelper
    {
        MemoryStream NormalizeOrientation(Stream imageStream, string imageExtension, bool removeExifOrientationTag = true);
        bool IsFileImage(byte[] fileBytes);
    }
}
