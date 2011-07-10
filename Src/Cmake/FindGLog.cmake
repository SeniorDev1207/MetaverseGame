FIND_PATH(GLOG_INCLUDE_DIR glog/logging.h)

# Google's provided vcproj files generate libraries with a "lib"
# prefix on Windows
IF(WIN32)
    set(GLOG_ORIG_FIND_LIBRARY_PREFIXES "${CMAKE_FIND_LIBRARY_PREFIXES}")
    set(CMAKE_FIND_LIBRARY_PREFIXES "lib" "")
ENDIF()

FIND_LIBRARY(GLOG_LIBRARY NAMES glog
             DOC "The Google Glog Library"
             )

MARK_AS_ADVANCED(GLOG_INCLUDE_DIR GLOG_LIBRARY)

# Restore original find library prefixes
IF(WIN32)
    SET(CMAKE_FIND_LIBRARY_PREFIXES "${GLOG_ORIG_FIND_LIBRARY_PREFIXES}")
ENDIF()

INCLUDE(FindPackageHandleStandardArgs)
FIND_PACKAGE_HANDLE_STANDARD_ARGS(GLOG DEFAULT_MSG GLOG_LIBRARY GLOG_INCLUDE_DIR)

IF(GLOG_FOUND)
    SET(GLOG_INCLUDE_DIRS ${GLOG_INCLUDE_DIR})
    SET(GLOG_LIBRARIES    ${GLOG_LIBRARY})
ENDIF()