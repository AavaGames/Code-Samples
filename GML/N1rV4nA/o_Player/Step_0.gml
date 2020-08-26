onWall = false;
onRamp = false;
inAir = false;
yspeed = 0;
yy_offset_2 = 0;

if place_meeting(x,y+1,o_Wall)
{
	onWall = true;
	wasonGroundlastframe = 20;
}
if place_meeting(x,y+3,o_half_ramp)
{
	onRamp = true;
	wasonGroundlastframe = 20;
}
if !place_meeting(x,y+1,o_half_ramp) and !place_meeting(x,y+1,o_half_ramp)
{
	inAir = true;
}

#region input

//Input
if hascontrol = true
{
	gas_held = keyboard_check(vk_space);
	angle_up = keyboard_check(vk_left) or keyboard_check(ord("A"));
	angle_down = keyboard_check(vk_right) or keyboard_check(ord("D"));
}

#endregion

#region speedboost and penalties

collective_movespeed_change = 0;

if landing_boost > 0
{
	collective_movespeed_change += landing_boost/60;
	landing_boost --;
}
if speed_boost > 0
{
	collective_movespeed_change += speed_boost/60;
	speed_boost --;
}
if speed_penalty > 0
{
	collective_movespeed_change -= speed_penalty/60;
	speed_penalty --;
}

movespeed_cap = set_movespeed_cap + collective_movespeed_change
#endregion

#region input and movement

//Horizontal movement
if onWall
{
	if gas_held == 1
	{
		movespeed += acceleration;
	}
	else if gas_held == 0
	{
		movespeed = Approach(movespeed,0,(acceleration/2))
	}
	
}
else if onRamp
{
	if gas_held == 1
	{
		movespeed += acceleration;
	}
	else if gas_held == 0
	{
		movespeed = Approach(movespeed,0,(acceleration/2));
	}
	yspeed += movespeed*(27/90);
}
else if inAir
{
	landing = true;
	
	if wasonGroundlastframe > 0
	{
		airtime = movespeed*(27/90);
		wasonGroundlastframe --;
	}
	
	airtime += grav
	yspeed = airtime;
	
	movespeed = Approach(movespeed,0,air_drag)
	
	if yy_offset == 0
	{
		if angle <= 30 and angle >= -10
		{
			if angle_up == 1
			{
				angle += 1;
				yy_offset_2 += (2.8/2)*(2/3)
			}
			if angle_down == 1
			{
				angle -= 1;
				yy_offset_2 -= (2.8/2)*(2/3)
			}
		}
	}
}

//Horizontal Collision
if (place_meeting(x+movespeed, y, o_Wall))
{
	var onepixel = sign(movespeed);
	while(!place_meeting(x+onepixel, y, o_Wall)) x += onepixel;
	movespeed = 0;
}
if place_meeting(x+movespeed,y,o_half_ramp)
{
	var yplus = 0;
	var onepixel = sign(movespeed);
	while (place_meeting(x+movespeed,y-yplus,o_half_ramp) && yplus <=abs(5)) yplus +=1;
	if place_meeting(x+movespeed,y-yplus,o_half_ramp)
	{
		while(!place_meeting(x+onepixel,y,o_half_ramp)) x += onepixel;
		movespeed = 0;
	}
	else
	{
		y -= yplus;
	}
}

//Verticle Collision
if (place_meeting(x, y+yspeed*2, o_Wall))
{
	var onepixel = sign(yspeed);
	while(!place_meeting(x, y+onepixel, o_Wall)) y += onepixel;
	yspeed = 0;
}
if (place_meeting(x, y+yspeed, o_half_ramp))
{
	var onepixel = sign(yspeed);
	while(!place_meeting(x, y+onepixel, o_half_ramp)) y += onepixel;
	yspeed = 0;
}

//Clamp variables
movespeed = clamp(movespeed,0,movespeed_cap)
//Set movespeed
x += movespeed;
y -= yspeed;

//As long as i dont go underneath the ground this saves me from getting stuck in its
if y > 191.5
{
	y = 191.49;
}


global.movespeed = movespeed;

#endregion

#region animations

xx = x;
yy = y;
motionblur = movespeed*8;

if movespeed > 0
{
	image_speed = (movespeed*2.5)/10
}
else if movespeed == 0
{
	image_speed = 0;
}

angle_yyoffset_reset = 2.8*1.2

if onRamp and movespeed > 0
{
	if angle != 27
	{
		angle += 3;
		if angle > 27 angle = 27;
		yy_offset += angle_yyoffset_reset;
		//X for rival
	}
}
else if inAir and movespeed > 0
{
	if yy_offset > 0
	{
		yy_offset -= angle_yyoffset_reset
	}
}

if onWall and movespeed > 0
{
	if angle > 0
	{
		angle -= 3;
		if angle < 3 angle = 0;
	}
	else if angle < 0
	{
		angle += 3;
		if angle > -3 angle = 0;
	}
	else if yy_offset > 0
	{
		yy_offset -= 2.1
	}
	else if yy_offset < 0
	{
		yy_offset += 2.1
	}
	
	if yy_offset_2 > 0
	{
		yy_offset_2 -= (2.8/2)*(2/3)
	}
	else if yy_offset_2 < 0
	{
		yy_offset_2 += (2.8/2)*(2/3)
	}
}
angle = clamp(angle,-10,30)
yy_offset_2 += yy_offset
yy += yy_offset_2

#endregion

if wasonGroundlastframe > 0
{
	wasonGroundlastframe --;
}

if onWall == true and landing == true
{
	if angle < 5 and angle > -5
	{
		landing_boost = 120;
		movespeed += 1;
		if !audio_is_playing(sfx_angleboost) and global.sfx == true
		{
			audio_play_sound(sfx_angleboost, 8, false);
			audio_sound_pitch(sfx_angleboost, 0.8);
		}
	}
	landing = false;
}

#region SFX



if movespeed > 0
{
	if !audio_is_playing(sfx_player_engineloop) and global.sfx == true
	{
		audio_play_sound(sfx_player_engineloop, 10, true);
	}
}
if movespeed == 0 or global.sfx == false
{
	if audio_is_playing(sfx_player_engineloop)
	{
		audio_stop_sound(sfx_player_engineloop);
	}
}

#endregion