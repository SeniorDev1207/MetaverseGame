#ifndef PYTHON_PYTHON_ENTRY_H
#define PYTHON_PYTHON_ENTRY_H

#include <boost/noncopyable.hpp>
#include <boost/python.hpp>
#include "Python/PythonInit.h"

namespace Egametang {

class PythonEntry: private boost::noncopyable
{
private:
	PythonInit python_init_;

	boost::python::object main_ns_;

	boost::unordered_set<std::string> python_paths_;

	boost::unordered_set<std::string> python_modules_;

public:  // private
	bool PythonEntry::GetExecString(const std::string& main_fun, std::string& exec_string);

public:
	PythonEntry();

	void ImportPath(std::string& path);

	void ImportModule(std::string& module);

	template <typename T>
	void RegisterObjectPtr(std::string& name, T object_ptr);

	void Exec();
};

} // namespace Egametang

#endif // PYTHON_PYTHON_ENTRY_H
