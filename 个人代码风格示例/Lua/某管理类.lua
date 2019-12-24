--SweepHelperMan
--Abstract class SweepHelper

class "SweepHelper"(function(_ENV)
	
	__DebugArguments__{}
	function SweepHelper(self)
		self.isOn = false;
		self.unlockLv = 0;
		self.unlockDay = 0;
		self.unlockTask = 0;
		
		self.lockedDesc = "";
		self.unlockedDesc = "";
		self.title = "";
		
		self.uiData = nil;
		self.conf = nil;
	end
	
	__DebugArguments__{}
	function IsLocked(self)
		local lvOK = Global.GetHostPlayerLevel() >= self.unlockLv;
		local dayOK = self:IsDayOK();
		local taskOK = IsTaskOK();
		
		return lvOK and dayOK and taskOK
	end

	__DebugArguments__{}
	function IsDayOK(self)
		local serverOpenDay = TODO()
		local dayOK = serverOpenDay > self.unlockDay;
		return dayOK
	end

	__DebugArguments__{}
	function IsTaskOK(self)
		local taskOK = TODO()
		return taskOK
	end
	
	__DebugArguments__{RawBoolean}
	function SetOn(self, val)
		self.isOn = val;
	end
	
	__DebugArguments__{}
	function IsOn(self)
		return self.isOn;
	end
	
	__DebugArguments__{}
	function Execute(self)
		--Virtual
	end
)


class "SweepHelperSweeping"(function(_ENV)
	inherit (SweepHelper)
	
	__DebugArguments__{}
	function SweepHelperSweeping(self)
		self.isLinkOnly = false;
		self.linkID = 0;
	end
	
	--[override]
	__DebugArguments__{}
	function Execute(self)
		if (self.isLinkOnly) then
			Global.GetLinkMan():DoLink(self.linkID);
		end
	end
)

class "SweepHelperSpeeding"(function(_ENV)
	inherit (SweepHelper)
	
	__DebugArguments__{}
	function SweepHelperSpeeding(self)
		self.speedFactor = 1;
	end
	
)


class "SweepHelperMan"(function(_ENV)
	
	__DebugArguments__{}
	function SweepHelperMan(self)
		self.sweepList = List();
		self.speedList = List();
	end
	
	__DebugArguments__{}
	function InitSweepHelperMan(self)
		self.sweepList = List();
		self.speedList = List();
	end
	
	
)















