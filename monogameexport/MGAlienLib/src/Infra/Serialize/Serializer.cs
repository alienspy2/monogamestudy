using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace MGAlienLib
{
    public class DeserializeContext
    {
        public Dictionary<Guid, MGObject> objectPool = new ();
        public List<(Guid, MGObject, FieldInfo)> deferredObjectLink = new();
    }


    public static class Serializer
    {
        public static string SerializePrefab(GameObject rootObj)
        {
            var transforms = rootObj.GetComponentsInChildren<Transform>();

            var dict = new Dictionary<string, object>();

            foreach (var transform in transforms)
            {
                var obj = transform.gameObject;
                string title = $"{obj.GetType().FullName}:{obj.Id.ToString()}";
                dict[title] = Serialize(obj);

                var components = obj.internal_GetComponents();
                foreach (var component in components)
                {
                    string componentTitle = $"{component.GetType().FullName}:{component.Id.ToString()}";
                    dict[componentTitle] = Serialize(component);
                }
            }

            return Toml.FromModel(dict);
        }


        public static GameObject DeserializePrefab(string toml)
        {
            DeserializeContext context = new DeserializeContext();

            var dict = Toml.Parse(toml).ToModel<Dictionary<string, TomlTable>>();

            foreach (var kvp in dict)
            {
                var title = kvp.Key;
                if (title.Contains(":"))
                {
                    var split = title.Split(':');
                    var typeName = split[0];
                    var id = Guid.Parse(split[1]);
                    var value = kvp.Value;
                    context.objectPool.Add(id, Deserialize(context, typeName, value));
                }
                else
                {
                    Logger.Log($"Unknown object: {title}");
                }
            }

            // Resolve deferred object links
            foreach (var kvp in context.deferredObjectLink)
            {
                Guid id = kvp.Item1;
                var obj = kvp.Item2;
                var field = kvp.Item3;

                if (context.objectPool.ContainsKey(id))
                {
                    field.SetValue(obj, context.objectPool[id]);
                }
                else
                {
                    field.SetValue(obj, null);
                }
            }

            // Finalize all objects
            foreach (var obj in context.objectPool)
            {
                obj.Value.FinalizeDeserialize(context);
            }

            // Find root object
            GameObject rootObj = null;

            foreach (var obj in context.objectPool)
            {
                if (obj.Value is GameObject gameObject)
                {
                    if (gameObject.transform.parent == null)
                    {
                        rootObj = gameObject;
                    }
                }
            }

            return rootObj;
        }

        private static Dictionary<string, object> Serialize<T>(T obj) where T : MGObject
        {
            var dict = new Dictionary<string, object>();

            obj.GetReadyForSerialize();

            // Private 필드 수집
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(SerializeFieldAttribute)))
                {
                    var value = field.GetValue(obj);
                    
                    if (value == null) continue;
                    else
                    {
                        object DTO = value.GetType() switch
                        {
                            Type t when t.IsSubclassOf(typeof(MGObject)) => (value as MGObject).Id.ToString(),
                            Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) =>
                                SerializeList(field, value),
                            _ => SerializeValue(value.GetType(), field, value)
                        };

                        if (DTO != null)
                        {
                            dict[field.Name] = DTO;
                        }
                    }
                }
            }

            return dict;
        }

        private static MGObject Deserialize(DeserializeContext context, string typeName, TomlTable dict)
        {
            // 현재 어셈블리에서 시도
            Assembly asm = Assembly.GetExecutingAssembly();
            Type type = asm.GetType(typeName);

            if (type == null)
            {
                // 모든 어셈블리에서 찾기
                var asemblies = AppDomain.CurrentDomain.GetAssemblies();
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(typeName))
                    .FirstOrDefault(t => t != null);
            }

            var obj = (MGObject)Activator.CreateInstance(type);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (dict.ContainsKey(field.Name))
                {
                    var value = dict[field.Name];
                    if (field.FieldType.IsSubclassOf(typeof(MGObject)))
                    {
                        context.deferredObjectLink.Add((Guid.Parse(value.ToString()), obj, field));
                    }
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        field.SetValue(obj, DeserializeList(field, value));
                    }
                    else
                    {
                        field.SetValue(obj, DeserializeValue(field.FieldType, field, value));
                    }
                }
            }

            return obj;
        }

        public static object SerializeValue(Type type, FieldInfo field, object value)
        {
            return type switch
            {
                Type t when t == typeof(Rectangle) => SerializeRectangle(field, (Rectangle)value),
                Type t when t == typeof(RectangleF) => SerializeRectangleF(field, (RectangleF)value),
                Type t when t == typeof(Vector2) => SerializeVector2(field, (Vector2)value),
                Type t when t == typeof(Vector3) => SerializeVector3(field, (Vector3)value),
                Type t when t == typeof(Vector4) => SerializeVector4(field, (Vector4)value),
                Type t when t == typeof(Quaternion) => SerializeQuaternion(field, (Quaternion)value),
                Type t when t == typeof(Color) => SerializeColor(field, (Color)value),
                Type t when t == typeof(Guid) => SerializeGuid(field, (Guid)value),
                Type t when t == typeof(eAssetSource) => value.ToString(),
                Type t when t == typeof(eHAlign) => value.ToString(),
                Type t when t == typeof(eVAlign) => value.ToString(),
                Type t when t == typeof(eCanvasType) => value.ToString(),
                _ => value
            };

            // <-- 실패 : default 값 제거 시도.
            //object defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;

            //if (value == null) return null;

            //if (type == typeof(Rectangle))
            //{
            //    if ((Rectangle)value == (Rectangle)defaultValue) return null;
            //    return SerializeRectangle(field, (Rectangle)value);
            //}
            //else if (type == typeof(RectangleF))
            //{
            //    if ((RectangleF)value == (RectangleF)defaultValue) return null;
            //    return SerializeRectangleF(field, (RectangleF)value);
            //}
            //else if (type == typeof(Vector2))
            //{
            //    if ((Vector2)value == Vector2.Zero) return null;
            //    return SerializeVector2(field, (Vector2)value);
            //}
            //else if (type == typeof(Vector3))
            //{
            //    if ((Vector3)value == Vector3.Zero) return null;
            //    return SerializeVector3(field, (Vector3)value);
            //}
            //else if (type == typeof(Vector4))
            //{
            //    if ((Vector4)value == Vector4.Zero) return null;
            //    return SerializeVector4(field, (Vector4)value);
            //}
            //else if (type == typeof(Quaternion))
            //{
            //    if ((Quaternion)value == Quaternion.Identity) return null;
            //    return SerializeQuaternion(field, (Quaternion)value);
            //}
            //else if (type == typeof(Color))
            //{
            //    if ((Color)value == (Color)defaultValue) return null;
            //    return SerializeColor(field, (Color)value);
            //}
            //else if (type == typeof(Guid))
            //{
            //    return SerializeGuid(field, (Guid)value);
            //}
            //else if (type == typeof(eAssetSource) ||
            //    type == typeof(eHAlign) ||
            //    type == typeof(eVAlign) ||
            //    type == typeof(eCanvasType))
            //{
            //    return value.ToString();
            //}
            //else if (type == typeof(int))
            //{
            //    if ((int)value == 0) return null;
            //    return value;
            //}
            //else if (type == typeof(float))
            //{
            //    if ((float)value == 0f) return null;
            //    return value;
            //}
            //else if (type == typeof(double))
            //{
            //    if ((double)value == 0) return null;
            //    return value;
            //}
            //else if (type == typeof(string))
            //{
            //    if ((string)value == null) return null;
            //    return value;
            //}
            //else if (type == typeof(bool))
            //{
            //    if ((bool)value == false)
            //        return null;
            //}

            //return value;
        }

        public static object DeserializeValue(Type type, FieldInfo field, object value)
        {

            return type switch
            {
                Type t when t == typeof(Rectangle) => DeserializeRectangle(field, value),
                Type t when t == typeof(RectangleF) => DeserializeRectangleF(field, value),
                Type t when t == typeof(Vector2) => DeserializeVector2(field, value),
                Type t when t == typeof(Vector3) => DeserializeVector3(field, value),
                Type t when t == typeof(Vector4) => DeserializeVector4(field, value),
                Type t when t == typeof(Quaternion) => DeserializeQuaternion(field, value),
                Type t when t == typeof(Color) => DeserializeColor(field, value),
                Type t when t == typeof(Guid) => DeserializeGuid(field, value),
                Type t when t == typeof(eAssetSource) => Enum.Parse(typeof(eAssetSource), value.ToString()),
                Type t when t == typeof(eHAlign) => Enum.Parse(typeof(eHAlign), value.ToString()),
                Type t when t == typeof(eVAlign) => Enum.Parse(typeof(eVAlign), value.ToString()),
                Type t when t == typeof(eCanvasType) => Enum.Parse(typeof(eCanvasType), value.ToString()),
                _ => Convert.ChangeType(value, type)
            };
        }

        public static object SerializeRectangle(FieldInfo field, Rectangle value)
        {
            return new int[] { value.X, value.Y, value.Width, value.Height };
        }

        public static Rectangle DeserializeRectangle(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;

            return new Rectangle(
                Convert.ToInt32(tomlArray[0]),
                Convert.ToInt32(tomlArray[1]),
                Convert.ToInt32(tomlArray[2]),
                Convert.ToInt32(tomlArray[3])
            );
        }

        public static object SerializeRectangleF(FieldInfo field, RectangleF value)
        {
            return new float[] { value.X, value.Y, value.Width, value.Height };
        }

        public static RectangleF DeserializeRectangleF(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;

            return new RectangleF(
                Convert.ToSingle(tomlArray[0]),
                Convert.ToSingle(tomlArray[1]),
                Convert.ToSingle(tomlArray[2]),
                Convert.ToSingle(tomlArray[3])
            );
        }

        public static object SerializeVector2(FieldInfo field, Vector2 value)
        {
            return new float[] { value.X, value.Y };
        }

        public static Vector2 DeserializeVector2(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;
            return new Vector2(
                Convert.ToSingle(tomlArray[0]),
                Convert.ToSingle(tomlArray[1])
            );
        }

        public static object SerializeVector3(FieldInfo field, Vector3 value)
        {
            return new float[] { value.X, value.Y, value.Z };
        }

        public static Vector3 DeserializeVector3(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;
            return new Vector3(
                Convert.ToSingle(tomlArray[0]),
                Convert.ToSingle(tomlArray[1]),
                Convert.ToSingle(tomlArray[2])
            );
        }

        public static object SerializeVector4(FieldInfo field, Vector4 value)
        {
            return new float[] { value.X, value.Y, value.Z, value.W };
        }

        public static Vector4 DeserializeVector4(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;
            return new Vector4(
                Convert.ToSingle(tomlArray[0]),
                Convert.ToSingle(tomlArray[1]),
                Convert.ToSingle(tomlArray[2]),
                Convert.ToSingle(tomlArray[3])
            );
        }

        public static Quaternion DeserializeQuaternion(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;
            return new Quaternion(
                Convert.ToSingle(tomlArray[0]),
                Convert.ToSingle(tomlArray[1]),
                Convert.ToSingle(tomlArray[2]),
                Convert.ToSingle(tomlArray[3])
            );
        }

        public static object SerializeQuaternion(FieldInfo field, Quaternion value)
        {
            return new float[] { value.X, value.Y, value.Z, value.W };
        }

        public static object SerializeColor(FieldInfo field, Color value)
        {
            return new byte[] { value.R, value.G, value.B, value.A };
        }

        public static Color DeserializeColor(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;
            return new Color(
                Convert.ToByte(tomlArray[0]),
                Convert.ToByte(tomlArray[1]),
                Convert.ToByte(tomlArray[2]),
                Convert.ToByte(tomlArray[3])
            );
        }

        public static object SerializeGuid(FieldInfo field, Guid value)
        {
            return value.ToString();
        }

        public static Guid DeserializeGuid(FieldInfo field, object value)
        {
            return Guid.Parse(value as string);
        }

        private static object SerializeList(FieldInfo field, object value)
        {
            var list = value as IList;
            if (list == null) return null;

            var elementType = field.FieldType.GetGenericArguments()[0];
            var result = new List<object>();

            foreach (var item in list)
            {
                if (item == null) continue;
                result.Add(SerializeValue(elementType, field, item));
            }

            return result;
        }

        private static object DeserializeList(FieldInfo field, object value)
        {
            var tomlArray = value as TomlArray;
            if (tomlArray == null) return null;

            var elementType = field.FieldType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType) as IList;

            foreach (var item in tomlArray)
            {
                list.Add(DeserializeValue(elementType, field, item));
            }

            return list;
        }
    }
}
