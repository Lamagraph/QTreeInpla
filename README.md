# QTreeInpla

QuadTree linear algebra implementation in Inpla.

# How to run experiments

* Inpla uses recursion on AST creation, hence use `ulimit -s unlimited` to have unlimited stack

* Use [Lamagraph/inpla](https://github.com/Lamagraph/inpla)'s `experiments` branch for more interpreter memory

* Use `scripts/mtx_to_experiment.fsx` to create inpla script ready to be run

* Be patient. The main bottleneck is loading a large coordinate list

# How to run golden tests

Ensure dotnet is installed.

1. Clone patched Inpla repository at [Lamagraph/inpla](https://github.com/Lamagraph/inpla)
2. Compile to obtain Inpla executable
3. Make sure you are in the project directory (Inpla's `use` directives are relative to current working directory)
4. `dotnet fsi test.fsx -- $PATH_TO_INPLA_EXECUTABLE` or simply `./test.fsx $PATH_TO_INPLA_EXECUTABLE`
