# Effects.bin

Back to [Mafia data formats](DataFiles.md)

Effects serve for ... #TODO

# Structure

An *effects.bin* file consists of single *header* structure and multiple *effect
blocks*. The number of effect blocks is derived from the size of file that is defined in the header structure.

## Header
The following structure is placed in the beginning of the file:

| Type          | Name          | Description|
| ------------- |:-------------:| ----------:|
| uint16\_t     | magicByte     | Identification of effects.bin file type. Must contain value 0x64.     |
| uint16\_t     | fileSize      | The size of given file, in bytes. |


## Effect blocks
The structure takes 74 bytes. 

| Type          | Name          | Description|
| ------------- |:-------------:| ----------:|
| uint16\_t     | mSign         | TODO       |
| uint32\_t     | mSize         | TODO |
| float32\*16   | transformationMatrix | The last 4 floats stand for (vec3Position, w)|
| uint32\_t     | effectId      | Identifies effect in effects.tbl |


