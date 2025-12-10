using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace AnalyticaDocs.Util
{
    public static class SqlDbHelper
    {
        public static List<T> DataTableToList<T>(DataTable table) where T : new()
        {
            var list = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (DataRow row in table.Rows)
            {
                T obj = new T();

                foreach (DataColumn column in table.Columns)
                {
                    var prop = Array.Find(properties, p => p.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase));
                    if (prop != null && prop.CanWrite)
                    {
                        object value = row[column];
                        Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        object safeValue;

                        // Handle DateOnly and TimeOnly
                        if (propType == typeof(DateOnly))
                        {
                            safeValue = value != DBNull.Value
                                ? DateOnly.FromDateTime(Convert.ToDateTime(value))
                                : default;
                        }
                        else if (propType == typeof(DateOnly?))
                        {
                            safeValue = value != DBNull.Value
                                ? DateOnly.FromDateTime(Convert.ToDateTime(value))
                                : null;
                        }
                        else if (propType == typeof(TimeOnly))
                        {
                            safeValue = value != DBNull.Value
                                ? value is TimeSpan ts ? TimeOnly.FromTimeSpan(ts) : TimeOnly.FromDateTime(Convert.ToDateTime(value))
                                : default;
                        }
                        else if (propType == typeof(TimeOnly?))
                        {
                            safeValue = value != DBNull.Value
                                ? value is TimeSpan ts ? TimeOnly.FromTimeSpan(ts) : TimeOnly.FromDateTime(Convert.ToDateTime(value))
                                : null;
                        }


                        else
                        {
                            if (value != DBNull.Value)
                            {
                                safeValue = Convert.ChangeType(value, propType);
                            }
                            else
                            {
                                // If the property is a non-nullable value type, create a default instance.
                                // Otherwise, set it to null.
                                if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
                                {
                                    safeValue = Activator.CreateInstance(prop.PropertyType);
                                }
                                else
                                {
                                    safeValue = null;
                                }
                            }
                        }
                        prop.SetValue(obj, safeValue, null);
                    }
                }

                list.Add(obj);
            }

            return list;
        }
    }
}