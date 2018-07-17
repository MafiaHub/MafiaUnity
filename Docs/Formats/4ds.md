# Model 4DS

Back to [Mafia data formats](../DataFiles.md)

4DS model defines the geometry and special properties that provide the ability to visualize the mesh in a specific configuration
as well as act as a node in a scene graph other objects might depend on.

# Structure

This file consists of a header describing the model format, its version as well as the timestamp of last edit, following an array of materials and models, ending by 
a byte specifying whether the model makes use of 5DS animations or not.

## Header

The following structure is placed in the beginning of the file:

| Type      |     Name      |                   Description                    |
|:----------|:-------------:|:------------------------------------------------:|
| uint16\_t |   magicByte   |       Format signature with text: "4DS\0"        |
| uint16\_t |    version    | Model version, 0x1D (29) for PC version of Mafia |
| uint64\_t |   timestamp   |                                                  |
| uint16\_t | materialCount |                                                  |
| -         |   materials   |            N definitions of materials            |
| uint16\_t |   meshCount   |                                                  |
| -         |    meshes     |             N definitions of meshes              |

## Material block

Describes the surface of a geometry as well as rendering behavior:

| Type      |     Name      |                Description                |
|:----------|:-------------:|:-----------------------------------------:|
| uint32\_t |     flags     |              Material flags               |
| float * 3 | ambientColor  |        Ambient RGB color in shadow        |
| float * 3 | diffuseColor  |        Diffuse RGB color in light         |
| float * 3 | emissionColor | Emission RGB color emitted to environment |
| float     |    opacity    |        0.0: invisible, 1.0: opaque        |

This block is valid only when env mapping is used:

| Type     |      Name       |               Description                |
|:---------|:---------------:|:----------------------------------------:|
| float    | envTextureRatio | 0.0: diffuse only, 1.0: env texture only |
| uint8\_t |  envNameLength  |              Max 255 chars               |
| char *N  |     envName     |                Uppercase                 |

This block always follows:

| Type     |       Name        |  Description  |
|:---------|:-----------------:|:-------------:|
| uint8\_t | diffuseNameLength | Max 255 chars |
| char *N  |    diffuseName    |   Uppercase   |

This block is valid only when animated material is used:

| Type      |     Name      |           Description            |
|:----------|:-------------:|:--------------------------------:|
| uint32\_t | sequenceCount | Number of animation sequences`1` |
| uint16\_t |     unk0      |                                  |
| uint32\_t |   seqPeriod   |         Sequence period          |
| uint32\_t |     unk1      |                                  |
| uint32\_t |     unk2      |                                  |

* `1` Maximum number of sequences is 100, diffuse named in abc**N-1**.bmp format (e.g. abc00.bmp, abc01.bmp, ...) while alpha is named in abc+**N-1**.bmp format (e.g. abc+00.bmp, abc+01.bmp, ...), base name can be found in diffuseName field that was already read above.

### Material Flags

| Value      |                 Name                  |                 Description                 |
|:-----------|:-------------------------------------:|:-------------------------------------------:|
| 0x00040000 |            Diffuse texture            |                                             |
| 0x08000000 |               Coloring                |                   See `1`                   |
| 0x00800000 |              Mip mapping              |                   See `2`                   |
| 0x04000000 |       Animated diffuse texture        |                                             |
| 0x02000000 |        Animated alpha texture         |                                             |
| 0x10000000 |         Double-sided material         |                   See `3`                   |
| 0x00080000 |              Env texture              |          Glossy texture simulation          |
| 0x00000100 |      Basic env texture blending       |                   See `4`                   |
| 0x00000200 |  Multiplicative env texture blending  |                   See `5`                   |
| 0x00000400 |     Additive env texture blending     |                   See `6`                   |
| 0x00001000 | Use X axis for env texture reflection |       Should be used for Env mapping        |
| 0x00002000 | Use Y axis for env texture reflection |          Used for detail textures           |
| 0x00004000 | Use Z axis for env texture reflection |          Used for detail textures           |
| 0x00008000 |           Additional effect           |                   See `7`                   |
| 0x40000000 |             Alpha texture             |                   See `8`                   |
| 0x20000000 |               Color key               |                   See `9`                   |
| 0x80000000 |           Additive blending           | Diffuse texture is used as emission texture |

* `1` Valid only when diffuse texture is used, Coloring is used always otherwise. Coloring is applied for ambient, emission and diffuse colors
* `2` Mip mapping generates several textures of various sizes which are then used based on rendering distance.
* `3` Material is visible from both sides of a geometry. Note: Mafia's renderer uses primary side of normals for shading, regardless of this flag.
* `4` Strength can be modified by parameter, final shading is defined by interpolation between diffuse and environmental texture.
* `5` Strength can't be modified by parameter. Final shading is defined by multiplication of diffuse and environmental texture.
* `6` Strength can't be modified by parameter. Final shading is defined by addition of diffuse and environmental texture.
* `7` Set to 1 when one or more of the following flags are used.
* `8` Map in grayscale used for opacity, black - invisible, white - opaque.
* `9` One color is used as transparent (e.g. black or the first color in color table of indexed bitmap), with mip mapping enabled, transparent borders are blurred.

## Mesh block

Describes the mesh name, properties, it's transformation as well as its type and associated data:

| Type     |   Name   | Description |
|:---------|:--------:|:-----------:|
| uint8\_t | meshType |             |

TODO