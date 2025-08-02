root_dir := justfile_directory()
lox_dir  := root_dir / 'lox'
tool_dir := root_dir / 'tool'
tool_bin := tool_dir / "bin" / "Debug" / "net7.0" / "generate_ast"

generate-ast:
  cd {{tool_dir}} && dotnet build
  cd {{lox_dir}} && {{tool_bin}} ./

run-interpreter:
  cd {{lox_dir}} && dotnet run $(fd .lox | fzf --height=12)

build-interpreter:
  cd {{lox_dir}} && dotnet build

