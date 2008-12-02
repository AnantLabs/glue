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
def filereplace(pathspec, pattern, replacement)
  Dir.glob(pathspec) do |path|
    oldtext = File.open(path, 'r').read()
	newtext = oldtext.gsub(pattern, replacement)
	if (oldtext != newtext) 
	  File.open(path, 'w') { |f| f.write(newtext) } 
	end
  end
end

# subversion tag 
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
  filereplace "src/**/AssemblyInfo.cs", /Assembly(Informational)?Version(Attribute)?\(\"[0-9\.]+\"\)/, "Assembly\\1Version\\2(\"#$product_version\")"
  filereplace "src/**/AssemblyVersion.cs", /Assembly(Informational)?Version(Attribute)?\(\"[0-9\.]+\"\)/, "Assembly\\1Version\\2(\"#$product_version\")"
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
