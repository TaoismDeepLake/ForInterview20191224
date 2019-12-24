local version = "2.2.1"
print("OrgAnalyzer "..version);

hasWarnings = hasWarnings or false;
hasErrors = hasErrors or false;

local ruleList = {
	default = "br:ReadInt32();",
	bool = "br:ReadInt32();",
	int = "br:ReadInt32();",
	uint = "br:ReadUInt32();",
	float = "br:ReadSingle();",
	auto_level_mode = "br:ReadInt32();",--枚举是int，但是可以做uint处理
	prof_mask = "br:ReadUInt32();",--掩码是uint
	int64 = "CSharpUtil.ReadInt64FromBr(br)",
	--ReadUnicodeString(br, 16);
	--datapath:br:ReadInt32(), CSharpUtil.GetIconName(br:ReadInt32())
};

StrList = {};
local warningSuppressList = {};
local areaCount = 0;
--
function LineStruct(name, index)

	local ret = {};
	ret.name = name;
	ret.id = -1;
	ret.areaList = {};
	ret.checkedLayer = 0;
	ret.itemType = "Int";
	ret.areaListByLayer = {};--排在前面的最先循环。
	ret.firstSeen = index;
	ret.lastSeen = index;
	ret.defaultVal = "";
	
	ret.comment = "";
	
	return ret;
	
end

--DEBUG
function LineStructToString(line)

	local str = "{\n";

	if (line == nil) then 
		str = "{nil}"; 
		return str 
	end

	local str = "{\n";
	str = str..line.name.."("..StrList[line.id].."\n";
	str = str..line.itemType.."\n";
	str = str..tostring(line.firstSeen).."-"..tostring(line.lastSeen).."\n";
	
	for i = 1, #line.areaList do 
		str = str..AreaToString(line.areaList[i]);
	end
	
	str = str.."\n}\n"
	
	return str 
end

function IsLayerSortComplete(lineStruct)
	return lineStruct.checkedLayer < #lineStruct.areaList
end

LineStructList = {}
--local LineStrList = {};

function AddToLineList(struct)
	LineStructList[#LineStructList + 1] = struct;
	return;
end

local lineCount = 0;

local lastLine = nil;

--TODO:have not considered type line , default value and first lines.
function Parse(str)
	local ret = nil;--LineStruct("s_"..tostring(#LineStructList))

	lineCount = lineCount + 1;
	
	if (str == "" or str == nil) then
		logFile:write("\nParse NULL!".."\n")
		return nil 
	end;
	
	if (string.find(str, "ItemTp")) then
		logFile:write("Skip as A:"..str.."\n")
		return nil 
	end;--TODO
	
	if (string.find(str, "Item")) then
		logFile:write("Skip as B:"..str.."\n")
		return nil 
	end;--TODO
	
	if (string.find(str, "Version")) then
		logFile:write("Skip as C:"..str.."\n")
		return nil 
	end;--TODO
	
	
	
	if (string.find(str, "Type:") or string.find(str, "Int:")) then 
		logFile:write("[Info] Found a default value---") --[[skip a line]]
		local defaultVal = str;
		
		local temp = inFile:read();
		
		defaultVal = str .. "\\n" .. temp
		
		lastLine.defaultVal = defaultVal;
		
		logFile:write("Content----"..str.."|"..temp.."\n");
		return nil 
	end;--TODO
	
	logFile:write("Decoding:"..str.."\n")
	
	ret = LineStruct(str, lineCount);--to be changed
	
	str = string.gsub(str, "Item: ", "");
	str = string.gsub(str, "%%", "%%%%");
	
	for number in string.gmatch(str, "[1234567890]+") do
		value = tonumber(number);
		ret.areaList[#ret.areaList + 1] = Area(value);
		ret.areaList[#ret.areaList].line = ret;
		ret.areaList[#ret.areaList].firstSeen = lineCount;
		ret.areaList[#ret.areaList].firstMax = lineCount;
	end 
	
	strFormat = string.gsub(str, "[1234567890]+", "%%d");
	for i = 1, #StrList do
		if (strFormat == StrList[i]) then
			ret.id = i;	
			break;
		end
	end
	
	if (ret.id == -1) then
		--not found
		ret.id = #StrList + 1;
		StrList[#StrList + 1] = strFormat;
	end
	
	ret.name = "s_" .. tostring(ret.id);
	
	--print(LineStructToString(ret));
	lastLine = ret;
	
	return ret;
end

function AnalyzeType()

	local line = inFile:read();
	logFile:write("Read Typeline: "..line.."\n");
	if (string.find(line, "ItemTp")) then
		return string.gsub(line, "ItemTp: ", "")
	else
		return "int"
	end
end

--conduct this for each item in the template.
--type and default value needs to be ignored(TODO)
function ProcessTextLine(str)
	local struct = Parse(str)

	if (struct == nil) then return end;
	
	for i = 1,#LineStructList do
		local structInList = LineStructList[i];
		
		if struct.name == structInList.name then
			--same loop clause
			if (IsLayerSortComplete(structInList)) then
				--try to sort layers
				for k = 1,#(structInList.areaList) do
					local areaInList = structInList.areaList[k]
				
					if (areaInList.layer == -1) then
						
						if (areaInList.start < struct.areaList[k].curMax) then
							--increased
							if (areaInList.layer == -1) then
								local layer = structInList.checkedLayer + 1
								structInList.checkedLayer = layer;
				
								areaInList.layer = layer;
								
								logFile:write(string.format("[line %s sorted %s to %d]",structInList.name,
								AreaToString(areaInList),structInList.checkedLayer));
							end
						end
					end
				end
			end	
			
			--track and update max value
			for k = 1,#(structInList.areaList) do
				local areaInList = structInList.areaList[k]
				local newVal = struct.areaList[k].curMax;
				
				if (areaInList.curMax ~= newVal) then
					local delta = newVal - areaInList.curMax
					
					--logFile:write(string.format("%s increment = %d\n",AreaToString(areaInList) ,areaInList.increment or -999))
					if (areaInList.increment == nil) then
						--second element in a list
						areaInList.increment = delta;
						table.insert(areaInList.valList, newVal);
						logFile:write(string.format("%s increment = %d\n",AreaToString(areaInList) ,areaInList.increment))
					elseif (areaInList.increment < 0 or areaInList.increment ~= delta) then
						if (warningSuppressList[structInList.id] == nil) then
							local errMsg = (string.format("[Warning] Irregular loop detected : %s",StrList[structInList.id]));
							--此处警告可以判重，不要重复提示以免刷屏。
							logFile:write(errMsg.."\n");
							print(errMsg);
							warningSuppressList[structInList.id] = true;
						end
						
						local valList = areaInList.valList;
						local existing = false;
						for k,v in pairs(valList) do
							if (v == newVal) then
								existing = true;
								break;
							end
						end
						
						if (existing == false) then
							table.insert(areaInList.valList, newVal);
						end
						
						areaInList.isIrregular = true;
						structInList.comment = 	string.format("\t\t--[Warning] The loop seems to be broken.");
						hasWarnings = true;
					end
					
					areaInList.curMax = struct.areaList[k].curMax
					areaInList.firstMax = lineCount;
				end
				
				structInList.lastSeen = lineCount;
			end
			
			--release struct
			return;
		end
	end
	
	--a new lineStruct
	AddToLineList(struct);
	struct.itemType = AnalyzeType()
end

--------

function Area(start)
	areaCount = areaCount + 1; 
	
	local ret = {}

	ret.name = "area";--untitled
	ret.layer = -1;--unsorted
	ret.curMax = start;
	ret.start = start;
	ret.increment = nil;
	
	ret.index = areaCount;
	ret.name = "x_"..tostring(areaCount);
	ret.line = nil;
	
	ret.firstSeen = -1;
	ret.firstMax = -1;
	
	ret.valList = {start};
	
	ret.isIrregular = false;
	return ret;
end

function CheckAreaMatch(area1, area2)
	
	if (area1 == nil or area2 == nil) then return false end;
	
	--can be optimized by generating ID
	local result = (area1.curMax == area2.curMax) and
	(area1.start == area2.start) and
	(area1.layer == area2.layer) and 
	(not ((area1.firstMax < area2.firstSeen) or (area2.firstMax < area1.firstSeen)));--overlap

	
	logFile:write(string.format("%s=%s : %s\n",AreaToString(area1),AreaToString(area2),tostring(result)));
	
	return result;
	--(area1.layer == area2.layer) and
	
	
end

--DEBUG
function AreaToString(area)
	local str = "";

	if (area == nil) then 
		str = "{nil}"; 
		return str 
	end
	
	if (area.curMax == area.start) then
		str = string.format("{%s[%s]:%s}",area.name , area.layer, area.start);
	else
		str = string.format("{%s[%s]:%s ~ %s}",area.name ,area.layer, area.start, area.curMax);
	end
		
	return str;
end

--------


--local inFile = nil;

--first loop
function ScanLoop(_inFile)
	inFile = _inFile;
	local line = inFile:read();
	while(line and line ~= "")
	do
		logFile:write("\nProcess:"..line.."\n")
		ProcessTextLine(line);
		logFile:write("\nAfter Process:"..line.."\n")
		line = inFile:read();
	end
	print(string.format("[Info] ScanLoop get %d lines.\n",#LineStructList))
end

-------
function CheckOverlap(line1, line2)

	-- local val = not ((line1.lastSeen < line2.firstSeen) or (line2.lastSeen < line1.firstSeen));
	-- print(string.format("Overlap:%s, %d-%d, %d-%d \n", tostring(val), 
	-- line1.firstSeen, line1.lastSeen, line2.firstSeen, line2.lastSeen))

	return not ((line1.lastSeen < line2.firstSeen) or (line2.lastSeen < line1.firstSeen)); 
end

function SortLayerList()
	for i = 1,#LineStructList do
		local structInList = LineStructList[i];
		for x = 1, #structInList.areaList do
			local area = structInList.areaList[x];
			
			if (area.layer ~= -1) then
				area.layer = area.line.checkedLayer + 1 - area.layer
				structInList.areaListByLayer[area.layer] = area;
			end
		end
		logFile:write(string.format("Line:%s\n", LineStructToString(structInList)))
		
	end
end

function ResortLayerList()
	for i = 1,#LineStructList do
		local structInList = LineStructList[i];
		for x = 1, #structInList.areaList do
			local area = structInList.areaList[x];
			structInList.areaListByLayer[area.layer] = area;
		end
		logFile:write(string.format("Line:%s\n", LineStructToString(structInList)))
		
	end
end

--second loop
function MergeLoop()
	logFile:write("Phase 2 begins")

	SortLayerList();
	
	--Judge which are the same variable
	for i = 1,#LineStructList do
		local structInList = LineStructList[i];
		
		local nextStruct = LineStructList[i + 1];
		if (nextStruct == nil) then break end;
		
		if (CheckOverlap(structInList, nextStruct)) then 
			for x = 1, #structInList.areaList do
			for y = 1, #nextStruct.areaList do
			
				local areaA = structInList.areaList[x];
				local areaB = nextStruct.areaList[y];
			
				local layerA = areaA.layer
				local layerB = areaB.layer
			
				if (layerA == layerB and layerA > 0) then
					local layerMax = layerA
					local isSame = true;
					
					local layerCountA = structInList.checkedLayer;
					local layerCountB = nextStruct.checkedLayer;
				
					for L = 1, layerMax do
						if (not CheckAreaMatch(structInList.areaListByLayer[L], nextStruct.areaListByLayer[L])) then
						
							logFile:write("[Break]:"..AreaToString(structInList.areaListByLayer[L])
							.."vs"..AreaToString(nextStruct.areaListByLayer[L]).."\n");
						
							isSame = false;
							break;
						end
					end
				
					if (isSame) then
						logFile:write(string.format("Merged: %s[%d] => %s[%d] .\n\n", structInList.name, x, nextStruct.name, y));
						
						nextStruct.areaList[y] = structInList.areaList[x];
					end
				end

			end
			end
		else--not overlap
			logFile:write(string.format("failed to merge %s and %s because they dont overlap.\n", structInList.name, nextStruct.name));
		end 
	end
	--handle numbers appeared only once
	for i = 1,#LineStructList do
		local structInList = LineStructList[i];
		
		for x = 1, #structInList.areaList do
			local var = structInList.areaList[x];
			if (var.layer == -1) then --start == curMax
				structInList.checkedLayer = structInList.checkedLayer + 1;
				var.layer = structInList.checkedLayer;
				logFile:write(string.format("[ONCE line %s sorted %s to %d]",structInList.name,
								AreaToString(areaInList),structInList.checkedLayer));
			end
		end
		
	end
	
	ResortLayerList();
end

-------------------------------------------------------
local outFileAna = nil;
local tabCount = 0;

function WriteTab(fs)
	if fs == nil then fs = outFileAna; end

	for i = 1, tabCount do
		fs:write("\t");
	end
end

function WriteFor(var)
	if (var.curMax == var.start) then return end

	if (var.isIrregular) then
		WriteTab();
		outFileAna:write(string.format("local %sValues = {", var.name));
		for k,v in pairs(var.valList) do
			outFileAna:write(string.format("%s, ", v));
		end
		outFileAna:write("} --[Irregular loop]\n");
		
		WriteTab();
		outFileAna:write(string.format("for %s = %d, #%sValues, %d do\n", var.name, 1, var.name, 1))
	else
		WriteTab();
		outFileAna:write(string.format("for %s = %d, %d, %d do\n", var.name, var.start, var.curMax, var.increment or 1))
	end
	
	tabCount = tabCount + 1;
end

function WritePrint(line)
	WriteTab();	
	if (line.checkedLayer == 0) then 
		outFileAna:write("WriteItem(\""..StrList[line.id].."\", \""..line.itemType.."\", \""..line.defaultVal.."\")\n");
		return;
	end
	
	outFileAna:write("WriteItemWithIndex(\""..StrList[line.id].."\", \""..line.itemType.."\", \""..line.defaultVal.."\"");
	for i = 1, #line.areaList do
		local area = line.areaList[i];
		if (area.curMax == area.start) then
			outFileAna:write(", "..tostring(area.curMax));
		else
			if (area.isIrregular) then
				outFileAna:write(string.format(", %sValues[%s]", area.name, area.name));
			else
				outFileAna:write(", "..(area.name));
			end
			
		end
	end
	outFileAna:write(")"..line.comment.."\n");
end

function WriteEnd(var, fs)
	if fs == nil then fs = outFileAna; end
	if (var.curMax == var.start) then return end;
	tabCount = tabCount - 1;
	WriteTab(fs);
	fs:write("end\n");
	
end

function LineHasVariable(line, var)

	if (line == nil or var == nil) then return false end;

	if (var.curMax == var.start) then return true end;--optimization. 
	
	for y = 1, #line.areaList do
		if (line.areaList[y] == var) then
			return true;
		end
	end
	
	return false;
end

-------
--third loop
function WriteLoop(_outFileAna)
	outFileAna = _outFileAna;

	--print(_outFileAna)
	_outFileAna:write("--Org Manipulator "..version.."\n")
	_outFileAna:write("--Proudly presented by Hu Shenjia\n")
	
	for i = 1,#LineStructList do
		local structInList = LineStructList[i];
		
		local lastStruct = LineStructList[i - 1];
		
		local nextStruct = LineStructList[i + 1];
		
		for x = 1, #structInList.areaListByLayer do
			local var = structInList.areaListByLayer[x];
			if (not LineHasVariable(lastStruct, var)) then
				WriteFor(var);
			end
		end
		
		logFile:write(string.format("WriteLine:%s\n", LineStructToString(structInList)))
		WritePrint(structInList);
		
		for x = 1, #structInList.areaListByLayer do
			local var = structInList.areaListByLayer[x];
			if (not LineHasVariable(nextStruct, var)) then
				WriteEnd(var);
			end
		end
		
		-- print(LineStructToString(structInList))
	end
	print(string.format("[Info] WriteLoop wrote %d lines to RESULT.lua.\n",#LineStructList))
end

--4th loop
--Added in 1.7.2, writes the DataTypes.lua code for programmers
local outFileDataTypes = nil;

function WriteDataTypeLoop(_outFileAna)
	outFileDataTypes = _outFileAna;

	--print(_outFileAna)
	_outFileAna:write("--Org Manipulator 1.7.2\n")
	_outFileAna:write("--Proudly presented by Hu Shenjia\n")
	_outFileAna:write("--Paste this in DataTps.lua\n")
	_outFileAna:write("\n")
	_outFileAna:write("class \"AAAA_CONFIG\"(function()\n")
	_outFileAna:write("	inherit (DT_CONFIG)\n")
	_outFileAna:write("\n")
	_outFileAna:write("	function AAAA_CONFIG(self)\n")
	_outFileAna:write("		Super(self);\n")
	_outFileAna:write("	end\n")
	_outFileAna:write("\n")
	_outFileAna:write("	__DebugArguments__{_BinReader_}\n")
	_outFileAna:write("	function load_data(self, br)\n")
	_outFileAna:write("\n")
	
	tabCount = 2;
	
	for i = 1,#LineStructList do
		local structInList = LineStructList[i];
		
		local lastStruct = LineStructList[i - 1];
		
		local nextStruct = LineStructList[i + 1];
		
		for x = 1, #structInList.areaListByLayer do
			local var = structInList.areaListByLayer[x];
			if (not LineHasVariable(lastStruct, var)) then
				WriteForDataTypes(var, structInList.areaListByLayer[x - 1]);
			end
		end
		
		logFile:write(string.format("DataTypes. WriteLine:%s\n", LineStructToString(structInList)))
		WritePrintDataTypes(structInList);
		
		for x = 1, #structInList.areaListByLayer do
			local var = structInList.areaListByLayer[x];
			if (not LineHasVariable(nextStruct, var)) then
				WriteEnd(var, outFileDataTypes);
			end
		end
		
		-- print(LineStructToString(structInList))
	end
	
	_outFileAna:write("	end\n")
	_outFileAna:write("end)\n")
	
	print(string.format("[Info] WriteLoop wrote to DataTp.lua\n",#LineStructList))
end

function WriteForDataTypes(var, preVar)
	if (var.curMax == var.start) then return end
	
	if ((var.layer) == 1) then
		
		outFileDataTypes:write("\n");
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("self.data%s = {}\n", var.index, var.name))
		
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("for %s = %d, %d, %d do\n", var.name, var.start, var.curMax, var.increment or 1))
		tabCount = tabCount + 1;
		
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("self.data%s[%s] = {}\n", var.index, var.name))
		
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("local info%s = self.data%s[%s]\n\n", var.index, var.index, var.name))
	else
		outFileDataTypes:write("\n");
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("info%s.data%s = {}\n", preVar.index, var.index, var.name))
		
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("for %s = %d, %d, %d do\n", var.name, var.start, var.curMax, var.increment or 1))
		tabCount = tabCount + 1;
		
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("info%s.data%s[%s] = {}\n", preVar.index, var.index, var.name))
		
		WriteTab(outFileDataTypes);
		outFileDataTypes:write(string.format("local info%s = info%s.data%s[%s]\n\n", var.index, preVar.index, var.index, var.name))
	end
end

function GetString(typeStr)
	
	for k,v in pairs(ruleList) do
		if k == typeStr then
			return v;
		end
	end
	
	--not found
	return ruleList["default"] .. "--["..typeStr.."]";
end

function WritePrintDataTypes(line)--TODO
	WriteTab(outFileDataTypes);	
	
	if (#(line.areaList) == 0) then
		outFileDataTypes:write(string.format("self.var%0.2s = %s \t--%s\n", line.id, GetString(line.itemType), StrList[line.id]));
	else
		outFileDataTypes:write(string.format("info%s.var%0.2s = %s \t--%s\n", line.areaListByLayer[#line.areaListByLayer].index, line.id, GetString(line.itemType), StrList[line.id]));
	end
	--
end


------
