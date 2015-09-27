using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using Lecoati.uMirror.Core;

using Umbraco.Core.Logging;

namespace Handlers
{
    /// <summary>
    /// Summary description for AssemblyTest
    /// </summary>
    public class AssemblyTest : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            var s = new StringBuilder();

            IList<MethodInfo> result = new List<MethodInfo>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            try
            {

                List<Type> types = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    s.AppendLine(assembly.FullName);

                    foreach (Type t in assembly.GetTypes())
                    {
                        s.AppendLine("Type: " + t.Name);

                        if (t.IsSubclassOf(typeof(uMirrorExtension)))
                        {
                            if (!types.Exists(type => type.Equals(t)))
                            {
                                result =
                                    result.Concat(
                                        t.GetMethods()
                                            .Where(m => m.GetCustomAttributes(typeof(UMirrorProxy), false).Length > 0)
                                            .ToList()).ToList();
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                LogHelper.Error(this.GetType(), "Failed to get assembly types.", ex);
                s.AppendLine("Exception:" + ex.Message);
            }

            context.Response.Write(s.ToString());
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}