using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Web
{
    /// <summary>
    /// Use this attribute on public methods on a Controller if you do NOT want them to be available as an "action".
    /// 
    /// </summary>
    public class ForbiddenAttribute : Attribute
    {
    }
}
