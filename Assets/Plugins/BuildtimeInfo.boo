# from https://forum.unity.com/threads/build-date-or-version-from-code.59134/
macro build_buildtime_info():
    dateString = System.DateTime.Now.ToString()
    yield [|
        class BuildtimeInfo:
            static def DateTimeString() as string:
                return "${$(dateString)}"
    |]
 
build_buildtime_info