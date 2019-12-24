---------------------------------
--coloring
hasWarnings = hasWarnings or false;
hasErrors = hasErrors or false;
local colorText = "F";
local colorBG = "2";

function GetCMD()
	if (hasWarnings) then
		colorText = "E";
	end

	if (hasErrors) then
		colorBG = "4";
	end
	
	return "color "..colorBG..colorText;
end

function AutoColor()
	local cmd = GetCMD();
	--print(cmd);
	os.execute(cmd);
end

local processedFileCount = 0;
local function RecordCount()
	processedFileCount = processedFileCount + 1;
	if (processedFileCount == 10 or
	processedFileCount == 50 or
	((processedFileCount % 100) == 0)) then
		print("processed " .. processedFileCount .. "tmpls");
	end
end
--------------


local excludeRaw = 
{
	'self',
	'local',
	'function',
	'for',
	'end',
	'if',
	'then',
	'do',
	'else',
	'in',
	'true',
	'false',
	'__DebugArguments__',
	'_BinaryReader_',
	'CSharpUtil',
	'not',
}

local excludeDict = {};

function initExclude()
	for k,v in ipairs(excludeRaw) do
		excludeDict[v] = true;
	end
end

--first loop, for DataTypes
function scanLoop(_inFile)
	for line in _inFile:lines() do
		findVars(line);
	end
end

--second loop, for *.lua
function scanLoopMaterial(_inFile)
	for line in _inFile:lines() do
		findVarsExisting(line);
	end
end

local findList = {};--a cache
local vocabCount = 0;
function findVars(lineStr)
	--delete self.*
	-- local lineStr = string.gsub(lineStr, "self%.[%a_]+[%w_]*", "")
	for varName in string.gmatch(lineStr, "[%a_]+[%w_]*") do
		if excludeDict[varName] == nil then	--exclude unneeded word
			local oldVal = findList[varName]
			if oldVal ~= nil then
				findList[varName] = oldVal + 1;
			else
				findList[varName] = 1;
				vocabCount = vocabCount + 1;
				if (vocabCount % 100) == 0 then
					print("Found "..vocabCount.." words.\n");
				end
			end
			--print(varName);
		end
	end
end

local vocab = {};
function constructVocabulary()
	--construct a constant list for refering
	for k,v in pairs(findList) do
		vocab[k] = true;
		findList[k] = 0;
		--vocabCount = vocabCount + 1; 
	end
	print(string.format("Vocabulary has %s words in Total.\n", vocabCount));
end

function findVarsExisting(lineStr)
	--delete self.*
	local lineStr = string.gsub(lineStr, "self%.[%a_]+[%w_]*", "")

	for varName in string.gmatch(lineStr, "[%a_]+[%w_]*") do
		if vocab[varName] ~= nil then
			local oldVal = findList[varName] or 0;
			if oldVal ~= nil then
				findList[varName] = oldVal + 1;
			end
		end
	end
end

function printResult(_outFile)
	local printing = {}
	for k,v in pairs(findList) do
		table.insert(printing, {k,v});
	end

	table.sort(printing, function(word1, word2)
		return word1[2] > word2[2];
		end
	);
	_outFile:write("result = {\n");
	for k,v in pairs(printing) do
		_outFile:write(string.format('["%s"] = %s;\n', v[1], v[2]));
	end
	_outFile:write("}\n");
end

function printVocab(_outFile)
	local printing = {}
	for k,v in pairs(vocab) do
		table.insert(printing, {k,v});
	end

	-- table.sort(printing, function(word1, word2)
		-- return word1[2] > word2[2];
		-- end
	-- );
	_outFile:write("vocab = {\n");
	for k,v in pairs(printing) do
		_outFile:write(string.format('["%s"] = true;\n', v[1]));
	end
	_outFile:write("}\n");
end

function storeVocab()
	local path = "DataTypes.lua"
	
	inFile = io.open (path, "r");
	if (inFile == nil) then 
		print("Cannot read "..path);
		hasErrors = true;
		AutoColor();
		return;
	end
	
	path = "RESULT\\VOCAB.lua"

	vocabFile = io.open (path, "w+");
	if (vocabFile == nil) then 
		print("Cannot open ".. path);
		hasErrors = true;
		AutoColor();
		return;
	end
	
	initExclude();
	scanLoop(inFile);
	constructVocabulary(findList);
	printVocab(vocabFile);
	
	inFile:close();
	vocabFile:close();
end

--not used
local function main()
	local path = "INPUT\\OrgAnalyzer_2.lua"
	
	inFile = io.open (path, "r");
	if (inFile == nil) then 
		print("Cannot read "..path);
		hasErrors = true;
		AutoColor();
		return;
	end	

	path = "RESULT\\RESULT.lua"

	outFile = io.open (path, "w+");
	if (outFile == nil) then 
		print("Cannot open ".. path);
		hasErrors = true;
		AutoColor();
		return;
	end
	
	initExclude();
	scanLoop(inFile);
	printResult(outFile);
	AutoColor();
	
	inFile:close();
	outFile:close();
end

local function ProcessLua(luaFile)
	scanLoopMaterial(luaFile);
	luaFile:close();
end

--------------------
require"lfs"
local input_dir = "./INPUT"
local output_dir = "./RESULT"

local inFile = nil;
local outFile = nil;

function attrdir(path)
	for file in lfs.dir(path) do
		--print(file.."process begin");
		if file ~= "." and file ~= ".." then --过滤linux目录下的"."和".."目录
			local f = path.. '/' ..file
			local attr = lfs.attributes (f)
			
			if attr.mode == "directory" then
			
				-- local resultDir = string.gsub(f, input_dir, output_dir)
				-- resultDir = string.sub(resultDir, 3, string.len(resultDir))
				
				-- local tempDir, err = io.open(resultDir, "r")
				
				-- if (tempDir == nil) then
					-- resultDir = string.gsub(resultDir, "/", "\\")
					-- os.execute("@echo off");
					-- os.execute("mkdir "..resultDir);
					-- os.execute("@echo on");
				-- else
					-- tempDir:close();
				-- end
				
				attrdir(f) --如果是目录，则进行递归调用
			else
				if string.find(tostring(file), ".lua") then
					--print(f .. "  -->  " .. attr.mode)
					
					local _inFile = io.open(f);
					local outFileName = string.gsub(f, input_dir, output_dir)
					local _outFile = io.open(outFileName, "w");
					
					if (_inFile == nil) then
						print("Failed to read: "..f);
					elseif (_outFile == nil) then
						print("Failed to write: "..outFileName);
					else 
						ProcessLua(_inFile);
						RecordCount();
					end
				end
			end
		end
	end
end

--------------------
local function TrueMain()
	local path = "RESULT\\RESULT.lua"
	outFile = io.open (path, "w+");
	if (outFile == nil) then 
		print("Cannot open ".. path);
		hasErrors = true;
		AutoColor();
		return;
	end

	local timeStampStart = os.time();
	storeVocab();
	
	attrdir("./INPUT")
	printResult(outFile);
	AutoColor();
	
	local timeStampEnd = os.time();
	print(string.format("Processed %s files in %s seconds.", processedFileCount, timeStampEnd - timeStampStart));
end

TrueMain();