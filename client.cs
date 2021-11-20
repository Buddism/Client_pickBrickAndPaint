function retrieveFocalPoint(%eval_callback)
{
	$retrieveFocalPoint_callback[$retrieveFocalQueueCount + 0] = %eval_callback;
	$retrieveFocalQueueCount++;
	commandToServer('setFocalPoint');
}

package retrieveFocalPoint
{
	function clientCmdSetFocalPoint(%point)
	{
		//$focalPoint = %point;
		%ret =  parent::clientCmdSetFocalPoint(%point);
		if($retrieveFocalQueueCount > 0)
		{
			$retrieveFocalQueueCount--;
			eval($retrieveFocalPoint_callback[$retrieveFocalQueueCount]);

			deleteVariables("$retrieveFocalPoint_callback" @ $retrieveFocalQueueCount);
		}
		return %ret;
	}
};
activatePackage(retrieveFocalPoint);

function pickBrickPlayer(%bool, %focalPoint_pass)
{
	if(%bool)
	{
		$pickBrickPlayer_activateTime = $Sim::Time;
		return;
	} else {
		if(!%focalPoint_pass)
			$pickBrickPlayer_pressTime = $Sim::Time - $pickBrickPlayer_activateTime;
	}

	if(!%focalPoint_pass)
		return retrieveFocalPoint("pickBrickPlayer(0, 1);");

	if(!isObject(%client = serverConnection) || !isObject(%player = %client.getControlObject()))
		return;

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

	if($pickBrickPlayer_pressTime > 0.12)
	{
		PaintGui_selectID(%brick.getColorID());
	} else 
		commandToServer('instantUseBrick', %brick.getDatablock());
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

