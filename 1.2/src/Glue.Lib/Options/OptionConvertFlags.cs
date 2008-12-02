using System;

namespace Glue.Lib.Options
{
    /// <summary>
    /// Summary description for OptionConvertFlags.
    /// </summary>
    public enum OptionConvertFlags
    {
        Normal              = 0,
        Silent              = 15,
        IgnoreUnknown       = 1,
        IgnoreMissing       = 2,
        IgnoreConvertErrors = 4,
        CaseSensitive       = 16,
        AttributedOnly      = 32,
        IncludeMethods      = 64,
        EatOptions          = 128
    }
}
