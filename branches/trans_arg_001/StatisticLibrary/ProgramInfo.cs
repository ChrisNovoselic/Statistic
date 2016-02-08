using System;
using System.Reflection;
using HClassLibrary;

namespace StatisticCommon
{
    /// <summary>
    /// Класс для создания спец имени для мьютекса
    /// </summary>
    static public class ProgramInfo
    {
        /// <summary>
        /// Создает уникальный идентификатор
        /// </summary>
        static public string AssemblyGuid
        {
            get
            {
                string nameGUID;

                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                if (attributes.Length == 0)
                    return String.Empty;

                nameGUID = ((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;

                object[] attributesTitle = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributesTitle[0];
                    if (titleAttribute.Title != "")
                        nameGUID = nameGUID + " "+ titleAttribute.Title;
                }
                //return 
                else
                    nameGUID = nameGUID + " " +System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);

                return nameGUID;
                    //((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;
            }
        }

        static public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                        return titleAttribute.Title;
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
        }
    }
}

