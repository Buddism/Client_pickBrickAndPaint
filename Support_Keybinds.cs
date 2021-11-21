//----------------------------------------------------------------------
// Title:   Support_Keybinds
// Author:  Greek2me
// Version: 1
// Updated: September 2, 2014
//----------------------------------------------------------------------
// Allows for easy addition and removal of keybinds.
//----------------------------------------------------------------------
// Include this code in your own scripts as an *individual file*
// called "Support_Keybinds.cs". Do not modify this code.
//----------------------------------------------------------------------

if($Support_Keybinds::Version >= 1 && !$Debug)
	return;
$Support_Keybinds::Version = 1;

function registerKeyBind(%div, %name, %cmd, %device, %action, %overWrite)
{
	%divIndex = -1;
	for(%i = 0; %i < $RemapCount; %i ++)
	{
		if($RemapDivision[%i] $= %div)
		{
			for(%e = %i; %e < $RemapCount; %e ++)
			{
				if($RemapDivision[%e] !$= $RemapDivision[%i] && $RemapDivision[%e] !$= "")
				{
					break;
				}

				if($RemapName[%e] $= %name)
				{
					return;
				}
			}
			%divIndex = %i;
			break;
		}
	}

	if(%divIndex >= 0)
	{
		for(%i = $RemapCount - 1; %i > %divIndex; %i --)
		{
			$RemapDivision[%i + 1] = $RemapDivision[%i];
			$RemapName[%i + 1] = $RemapName[%i];
			$RemapCmd[%i + 1] = $RemapCmd[%i];
		}

		$RemapDivision[%divIndex + 1] = "";
		$RemapName[%divIndex + 1] = %name;
		$RemapCmd[%divIndex + 1] = %cmd;
		$RemapCount ++;
	}
	else
	{
		$RemapDivision[$RemapCount] = %div;
		$RemapName[$RemapCount] = %name;
		$RemapCmd[$RemapCount] = %cmd;
		$RemapCount ++;
	}

	if(%device !$= "" && %action !$= "")
	{
		if(moveMap.getCommand(%device, %action) $= "" || %overWrite)
			moveMap.bind(%device, %action, %cmd);
	}
}

function unRegisterKeyBind(%div, %name, %cmd)
{
	%binding = moveMap.getBinding(%cmd);
	%device = getField(%binding, 0);
	%action = getField(%binding, 1);

	if(%device !$= "" && %action !$= "")
		moveMap.unBind(%device, %action);

	for(%i = 0; %i < $RemapCount; %i ++)
	{
		%d = $RemapDivision[%i];
		%n = $RemapName[%i];
		%c = $RemapCmd[%i];

		%start = 0;

		if(%n $= %name && %c $= %cmd && (%d $= %div || %lastDiv $= %div))
		{
			%start = %i + 1;
			break;
		}

		if(%d !$= "")
			%lastDiv = %d;
	}

	if(%start > 0)
	{
		for(%i = %start; %i < $RemapCount; %i ++)
		{
			$RemapDivision[%i - 1] = $RemapDivision[%i];
			$RemapName[%i - 1] = $RemapName[%i];
			$RemapCmd[%i - 1] = $RemapCmd[%i];
		}

		%d = $RemapDivision[%start - 1];
		if(%d $= "")
			$RemapDivision[%start - 1] = %div;

		$RemapDivision[$RemapCount - 1] = "";
		$RemapName[$RemapCount - 1] = "";
		$RemapCmd[$RemapCount - 1] = "";

		$RemapCount --;
	}
}