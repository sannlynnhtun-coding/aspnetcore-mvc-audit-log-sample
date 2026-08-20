using System.Reflection;
using AuditLogSample.Models;

namespace AuditLogSample.Services;

public static class ObjectModifier
{
    public static List<ChangeRecord> TrackChanges<T1, T2>(this T1 obj1, T2 obj2, bool isTrackAll = false)
        where T1 : class
        where T2 : class
    {
        var changeLog = new List<ChangeRecord>();

        var properties1 = typeof(T1).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property1 in properties1)
        {
            if (!property1.CanWrite) continue;

            var property2 = typeof(T2).GetProperty(property1.Name, BindingFlags.Public | BindingFlags.Instance);
            if (property2 == null || !property2.CanRead) continue;

            var oldValue = property1.GetValue(obj1);
            var newValue = property2.GetValue(obj2);

            if (isTrackAll)
            {
                changeLog.Add(new ChangeRecord
                {
                    FieldName = property1.Name,
                    OldValue = oldValue,
                    NewValue = newValue
                });
                property1.SetValue(obj1, newValue);
            }
            else
            {
                if (oldValue?.ToString() != newValue?.ToString())
                {
                    if (newValue is null) continue;

                    changeLog.Add(new ChangeRecord
                    {
                        FieldName = property1.Name,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                    property1.SetValue(obj1, newValue);
                }
            }
        }

        return changeLog;
    }
}
