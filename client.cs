exec("./Support_Keybinds.cs");

//registerKeyBind("Slayer", "Options", "SlayerClient_pushOptions");


registerKeyBind("Brick Picker", "Pick Brick", "Picker_PickBrick");
registerKeyBind("Brick Picker", "Pick Color", "Picker_PickColor");
registerKeyBind("Brick Picker", "Pick Print", "Picker_PickPrint");

registerKeyBind("Brick Picker", "Pick Brick or Paint or Print", "Picker_PickBrickPaint");

if($Pref::PickBrick::TimeForPaint $= "")
	$Pref::PickBrick::TimeForPaint = 0.12; //in seconds (default value: 0.12)

if($Pref::PickBrick::TimeForPrint $= "")
	$Pref::PickBrick::TimeForPrint = 0.4; //in seconds (default value: 0.3)

//first argument will always be %focalPoint
function retrieveFocalPoint(%callback, %arg0, %arg1)
{
	if(!isFunction(%callback))
		return;
		
	%count = $retrieveFocalQueueCount + 0;

	$retrieveFocalPoint_callback[%count] = %callback;
	$retrieveFocalPoint_callbackArgs[%count, 0] = %arg0;
	$retrieveFocalPoint_callbackArgs[%count, 1] = %arg1;

	$retrieveFocalQueueCount++;
	commandToServer('setFocalPoint');
}

package retrieveFocalPoint
{
	function clientCmdSetFocalPoint(%point)
	{
		//$focalPoint = %point;
		%ret = parent::clientCmdSetFocalPoint(%point);
		if($retrieveFocalQueueCount > 0)
		{
			%callback = $retrieveFocalPoint_callback[0];
			if(isFunction(%callback))
				call(%callback, vectorScale(%point, 1), $retrieveFocalPoint_callbackArgs[0, 0], $retrieveFocalPoint_callbackArgs[0, 1]); //make sure its a vector3f

			%count = $retrieveFocalQueueCount;
			for(%i = 0; %i < %count - 1; %i++)
			{
				$retrieveFocalPoint_callback[%i]        = $retrieveFocalPoint_callback[%i + 1];
				$retrieveFocalPoint_callbackArgs[%i, 0] = $retrieveFocalPoint_callbackArgs[%i + 1, 0];
				$retrieveFocalPoint_callbackArgs[%i, 1] = $retrieveFocalPoint_callbackArgs[%i + 1, 1];
			}

			$retrieveFocalPoint_callback[%count - 1]        = "";
			$retrieveFocalPoint_callbackArgs[%count - 1, 0] = "";
			$retrieveFocalPoint_callbackArgs[%count - 1, 1] = "";
			$retrieveFocalQueueCount--;
		}
		return %ret;
	}
};
activatePackage(retrieveFocalPoint);

function Picker_HandleCallbacks(%point, %callback)
{
	%end = $focalpoint;
	if(%end $= "")
	{
		clientCmdCenterprint("\c0could not find brick", 1);
		return;
	}

	initClientBrickSearch(%end, "0.1 0.1 0.1");
	%brick = clientBrickSearchNext();
	$lastPickBrick = %brick;

	if(!isObject(%brick))
	{
		clientCmdCenterprint("\c0could not find brick", 1);
		return;
	}

	call(%callback, %brick, %point);
}

function Picker_PickBrick(%value)
{
	if(!%value)
		return;

	retrieveFocalPoint("Picker_HandleCallbacks", "Picker_PickBrick_Callback");
}

function Picker_PickColor(%value)
{
	if(!%value)
		return;

	retrieveFocalPoint("Picker_HandleCallbacks", "Picker_PickColor_Callback");
}

function Picker_PickPrint(%value)
{
	if(!%value)
		return;

	retrieveFocalPoint("Picker_HandleCallbacks", "Picker_PickPrint_Callback");
}

function Picker_PickBrickPaint(%value)
{
	if(%value)
	{
		$Picker_PaintBrickHoldTime = $Sim::Time;
		return;
	}

	%time = $Sim::Time - $Picker_PaintBrickHoldTime;

	if(%time > $Pref::PickBrick::TimeForPrint)
		Picker_PickPrint(1);
	if(%time > $Pref::PickBrick::TimeForPaint)
		Picker_PickColor(1);
	else 
		Picker_PickBrick(1);
}

function Picker_PickBrick_Callback(%brick, %point)
{
	BSD_RightClickIcon(%brick.getDatablock());
}

function Picker_PickColor_Callback(%brick, %point)
{
	PaintGui_selectID(%brick.getColorID());
}

function Picker_PickPrint_Callback(%brick, %point)
{
	PSD_Click(%brick.getPrintID());
}


function PaintGui_selectID(%colorID)
{
	if(%colorID > 64)
		return;

	%divisions = $Paint_NumPaintRows - 1; // subtract the fxRow
	for(%I = 0; %I < %divisions; %I++)
	{
		%div = getSprayCanDivisionSlot(%I)+1;
		if(%colorID < %div)
		{
			%best = %I;
			%bestDiv = %lastDiv;
			break;
		}
		%lastDiv = %div;
	}

	if(%colorID >= %div)
		return 0;

	if(%best $= "")
		return;

	$CurrPaintSwatch = %colorID - %bestDiv;
	if(HUD_PaintActive.isVisible())
		PlayGui.FadePaintRow($CurrPaintRow);

	$CurrPaintRow = %best;

	if(HUD_PaintActive.isVisible())
		PlayGui.UnFadePaintRow($CurrPaintRow);

	PlayGui.updatePaintActive();

	$currSprayCanIndex = %colorID;
	commandToServer('useSprayCan', %colorID);

	if ($RecordingBuildMacro && isObject ($BuildMacroSO))
		$BuildMacroSO.pushEvent ("Server", 'useSprayCan', %canIndex);

	if(!$pref::Hud::RecolorBrickIcons)
		return;

	%color = getColorIDTable($currSprayCanIndex);
	%color = getWords(%color, 0, 2) SPC mClampF(getWord(%color, 3), 0.1, 1);
	for(%I = 0; %i < $BSD_NumInventorySlots; %I++)
		if(isObject($HUD_BrickIcon[%i]))
			$HUD_BrickIcon[%i].setColor(%color);

	//would support TMBI color stuff but its annoying
} 

