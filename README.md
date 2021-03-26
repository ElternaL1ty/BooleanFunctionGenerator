# BooleanFunctionGenerator
BooleanFunctionMinimizer contains several classes that allow you to set and minimize boolean functions using known methods.
## Installation
Download and extract .ZIP folder and copy BooleanFunctionMinimizer.cs to your project folder.
## Usage
BooleanFunctionMinimizer provides several methods to set and minimize functions that will be explained there
### Quine–McCluskey alghorithm (QMC)
QMC method is setting a minimized boolean function from truth table, that goes by input. The main idea is to group similar prime implicants and build a table, that shows the covering of implicants.

**Initialize a QMC object**
```c#
public class QMC(string[] vars, bool t = true)
```
Where
- * *string[] vars* * is string array with names of boolean function variables. **Required**
- * *bool t* * indicates whether truth table will be printed . **Optional. Default: true**

**Generate function**
```c#
public string Generate(int[] f_values, bool truth_table = false, bool const_groups = false, bool paste_groups = false, bool coating_table = false, bool answer = false)
```
Where
- *int[] f_values* are values of boolean functions to respective implicants. **Required**
- *bool truth_table* indicates whether truth table will be printed. **Optional. Default: false**
- *bool const_groups* indicates whether implicant groups will be printed. **Optional. Default: false**
- *bool paste_groups* indicates whether size-2 implicant groups will be printed. **Optional. Default: false**
- *bool coating_table* indicates whether cover table will be printed. **Optional. Default: false**
- *bool answer* indicates whether answer will be printed. **Optional. Default: false**

If something need to be printed, but don't exist in current function, this will be printed mannualy
No matter what, function will be returned in string format and you can get it for further proccess
## Example of usage
Here you can see live example of usage QMC method
```c#
// Initialize QMC object with variable array
// no bool t initialized, so by default, this will print out truth table
FunctionMinimizer.QMC t = new FunctionMinimizer.QMC(new string[] { "a", "b", "c" });

//Calling .Generate method with function values
//answer:true so function will be printed
//everything else will not be printed by default
//
string s = t.Generate(new int[] { 1, 1, 0, 0, 1, 0, 0, 1 }, answer:true);
//
//Some operations with s
//
c.ReadKey();
```
## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
## License
[MIT](https://choosealicense.com/licenses/mit/)
