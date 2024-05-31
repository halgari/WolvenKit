using System;

namespace WolvenKit.Core.Attributes;

public class BitfieldLength(uint length) : Attribute
{
    public uint Length { get; } = length;
}