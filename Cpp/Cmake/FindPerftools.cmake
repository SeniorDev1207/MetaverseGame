FIND_PATH(PERFTOOLS_INCLUDE_DIR google/heap-profiler.h)

IF(WIN32)
	set(CMAKE_FIND_LIBRARY_PREFIXES "lib" "")
	FIND_LIBRARY(PERFTOOLS_DEBUG_LIBRARY NAMES tcmalloc_minimal-debug
		DOC "The Google Perftools Debug Library"
		)
ELSEIF(UNIX)
	FIND_LIBRARY(PERFTOOLS_DEBUG_LIBRARY NAMES tcmalloc_debug
		DOC "The Google Perftools Debug Library"
		)
ENDIF()

FIND_LIBRARY(PERFTOOLS_LIBRARY NAMES tcmalloc
			DOC "The Google Perftools Library"
			)

FIND_LIBRARY(PERFTOOLS_PROFILE_LIBRARY NAMES profiler
			DOC "The Google Perftools Profile Library"
			)

MARK_AS_ADVANCED(PERFTOOLS_INCLUDE_DIR PERFTOOLS_DEBUG_LIBRARY PERFTOOLS_LIBRARY PERFTOOLS_PROFILE_LIBRARY)

INCLUDE(FindPackageHandleStandardArgs)
FIND_PACKAGE_HANDLE_STANDARD_ARGS(PERFTOOLS DEFAULT_MSG 
    PERFTOOLS_LIBRARY PERFTOOLS_DEBUG_LIBRARY PERFTOOLS_PROFILE_LIBRARY 
    PERFTOOLS_INCLUDE_DIR)

IF(PERFTOOLS_FOUND)
	SET(PERFTOOLS_INCLUDE_DIRS ${PERFTOOLS_INCLUDE_DIR})
	SET(PERFTOOLS_DEBUG_LIBRARIES ${PERFTOOLS_DEBUG_LIBRARY})
	SET(PERFTOOLS_LIBRARIES ${PERFTOOLS_LIBRARY})
	SET(PERFTOOLS_PROFILE_LIBRARIES ${PERFTOOLS_PROFILE_LIBRARY})
ENDIF()