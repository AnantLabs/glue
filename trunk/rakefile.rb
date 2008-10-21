$product_name    = "Glue"
$product_version = "0.9.10"
$dotnet_path     = "c:/windows/microsoft.net/framework/v2.0.50727"
$ignore_files    = ["*.config", "*.cs", "*.resx", "*.sln", "*.suo", "*.build", "*.pdb", "*.csproj", "*.user", ".svn", "obj", "Debug", "Release", "Code"]

# Invoke msbuild
def msbuild(solution, options = {})
  options[:target] ||= "build"
  options[:configuration] ||= "release"
  options[:targetversion] ||= "2.0"
  sh "#$dotnet_path/msbuild.exe #{solution} /v:minimal /t:#{options[:target]} /p:Configuration=#{options[:configuration]};TargetFrameworkVersion=#{options[:targetversion]}"
end

# Invoke unison
def unison(source, dest, options = {})
  cmd = "unison.exe #{source} #{dest}"
  options[:ignore] ||= []
  options[:ignore].each do |s|
    cmd << " -ignore \"Name #{s}\""
  end
  sh cmd
end

# Perform regex replace in file
def filegsub(path, pattern, replacement)
  text = File.readlines(path).join
  text = text.gsub(pattern, replacement)
  puts "============="
  puts "File: #{path}"
  puts "============="
  puts text
  puts
end

# set version in assemblyinfo.cs files
def set_version(pathspec, version)
  Dir.glob(pathspec).each do |path|
    filegsub path, /AssemblyVersion\("[^"]+"\)/, "AssemblyVersion(\"#{version}\")"
  end
end

def tag(version)
  output = `svn info` 
  output =~ /^URL\: (.*$)/
  trunk = $1
  tag = trunk.gsub "trunk/#$product_name", "tags/#$product_name/#{version}"
  puts "copy #{trunk} to #{tag}"
end

task :default => [:clean,:build]
task :all => [:clean,:build,:test]

desc "Clean all build outputs"
task :clean do
  msbuild "src/#$product_name.sln", :target=>"clean"
end

desc "Build project"
task :build do
  # set_version "src/*/AssemblyInfo.cs", "1.1"
  msbuild "src/#$product_name.sln", :target=>"build"
end

desc "Perform unit tests"
task :test do
  tag $product_version
end

desc "Deploy to preview site"
task :preview do
  # unison "Target-Preview",  $preview_path, :ignore => $ignore_files
end

desc "Deploy to production site"
task :deploy do
  # unison "Target-Production",  $staging_path, :ignore => $ignore_files
end
