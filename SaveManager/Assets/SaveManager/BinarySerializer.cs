using System.IO;
using System;
using UnityEngine;


public static class BinarySerializer
{
    private enum TypeCode : byte
    {
        Null = 0,
        Int,
        Float,
        Bool,
        String,
        Vector3,
        Array,
        Object
    }

    public static void WriteObject(BinaryWriter writer, object obj)
    {
        if (obj == null)
        {
            writer.Write((byte)TypeCode.Null);
            return;
        }

        Type type = obj.GetType();

        // primitives
        if (type == typeof(int))
        {
            writer.Write((byte)TypeCode.Int);
            writer.Write((int)obj);
        }
        else if (type == typeof(float))
        {
            writer.Write((byte)TypeCode.Float);
            writer.Write((float)obj);
        }
        else if (type == typeof(bool))
        {
            writer.Write((byte)TypeCode.Bool);
            writer.Write((bool)obj);
        }
        else if (type == typeof(string))
        {
            writer.Write((byte)TypeCode.String);
            writer.Write((string)obj);
        }
        else if (type == typeof(Vector3))
        {
            Vector3 v = (Vector3)obj;
            writer.Write((byte)TypeCode.Vector3);
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }
        else if (type.IsArray)
        {
            Array arr = (Array)obj;

            writer.Write(type.GetElementType().AssemblyQualifiedName);

            writer.Write((byte)TypeCode.Array);

            int rank = arr.Rank;
            writer.Write(rank);

            // Write dimensions of array
            for (int d = 0; d < rank; d++)
            {
                writer.Write(arr.GetLength(d));
            }

            // Write all elements in linear order
            foreach (var item in arr)
            {
                WriteObject(writer, item);
            }
        }
        else // custom class/struct
        {
            writer.Write((byte)TypeCode.Object);

            var fields = type.GetFields(System.Reflection.BindingFlags.Public |
                                       System.Reflection.BindingFlags.NonPublic |
                                       System.Reflection.BindingFlags.Instance);

            writer.Write(type.AssemblyQualifiedName);
            writer.Write(fields.Length);

            foreach (var field in fields)
            {
                writer.Write(field.Name);
                WriteObject(writer, field.GetValue(obj));
            }
        }
    }

    public static object ReadObject(BinaryReader reader)
    {
        TypeCode code = (TypeCode)reader.ReadByte();

        switch (code)
        {
            case TypeCode.Null:
                return null;

            case TypeCode.Int:
                return reader.ReadInt32();

            case TypeCode.Float:
                return reader.ReadSingle();

            case TypeCode.Bool:
                return reader.ReadBoolean();

            case TypeCode.String:
                return reader.ReadString();

            case TypeCode.Vector3:
                return new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                );

            case TypeCode.Array:
                string elementTypeName = reader.ReadString();
                Type elementType = Type.GetType(elementTypeName);

                int rank = reader.ReadInt32();

                int[] lengths = new int[rank];
                int totalLength = 1;

                for (int i = 0; i < rank; i++)
                {
                    lengths[i] = reader.ReadInt32();
                    totalLength *= lengths[i];
                }

                // Read flat values
                object[] flat = new object[totalLength];

                for (int i = 0; i < totalLength; i++)
                {
                    flat[i] = ReadObject(reader);
                }

                // Rebuild array using reflection
                Array arr = Array.CreateInstance(elementType, lengths);

                // Fill using indices
                int[] indices = new int[rank];

                for (int i = 0; i < totalLength; i++)
                {
                    GetIndices(i, lengths, indices);
                    arr.SetValue(flat[i], indices);
                }

                return arr;

            case TypeCode.Object:
                {
                    string typeName = reader.ReadString();
                    Type type = Type.GetType(typeName);

                    object obj = Activator.CreateInstance(type);

                    int fieldCount = reader.ReadInt32();

                    for (int i = 0; i < fieldCount; i++)
                    {
                        string fieldName = reader.ReadString();
                        var field = type.GetField(fieldName,
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        object value = ReadObject(reader);
                        field.SetValue(obj, value);
                    }

                    return obj;
                }
        }

        return null;
    }

    private static void GetIndices(int flatIndex, int[] lengths, int[] indices)
    {
        for (int i = lengths.Length - 1; i >= 0; i--)
        {
            indices[i] = flatIndex % lengths[i];
            flatIndex /= lengths[i];
        }
    }
}
