using System;

namespace echo
{
    internal record Item(ReadOnlyMemory<byte> Data, string ContentType);
}
