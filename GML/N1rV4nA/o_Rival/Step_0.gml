onWall = false;
onRamp = false;
inAir = false;
yspeed = 0;

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

if x+1000 < o_Player.x
{
	movespeed_cap = set_movespeed_cap + collective_movespeed_change + 2
}
else if x-500 > o_Player.x
{
	movespeed_cap = set_movespeed_cap + collective_movespeed_change - 2
	go = true;
}
else 
{
	movespeed_cap = set_movespeed_cap + collective_movespeed_change;
}

#endregion

#region input and movement

//Horizontal movement
if onWall
{
	if go == 1
	{
		movespeed += acceleration;
	}
	else if go == 0
	{
		movespeed = Approach(movespeed,0,(acceleration/2))
	}
	
}
else if onRamp
{
	if go == 1
	{
		movespeed += acceleration;
	}
	else if go == 0
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
		
		adjust_angle_timer = 30;
		
		wasonGroundlastframe --;
	}
	
	airtime += grav
	yspeed = airtime;
	
	movespeed = Approach(movespeed,0,air_drag)
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
yy = y-2;
motionblur = movespeed*8;

if movespeed > 0
{
	image_speed = ((movespeed*1.052)*2.5)/10
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

if inAir and adjust_angle_timer == 1
{
	if angle > -3
	{
		angle -= 1;
		//if angle < 3 angle = 0;
	}
	else if angle < -3
	{
		angle += 1;
		//if angle > -3 angle = 0;
	}
	else if yy_offset > 0
	{
		yy_offset += (2.8/2)*(2/3)
	}
	else if yy_offset < 0
	{
		yy_offset += (2.8/2)*(2/3)
	}
	else if adjust_angle_timer == 1
	{
		adjust_angle_timer = 0;
	}
}
else if onWall and movespeed > 0 
{
	if angle > 0
	{
		angle -= 1;
		if angle < 3 angle = 0;
	}
	else if angle < 0
	{
		angle += 1;
		if angle > -3 angle = 0;
	}
	else if yy_offset > 0
	{
		yy_offset += (2.8/2)*(2/3)
	}
	else if yy_offset < 0
	{
		yy_offset += (2.8/2)*(2/3)
	}
}

angle = clamp(angle,-10,30)
yy += yy_offset

#endregion

if wasonGroundlastframe > 0
{
	wasonGroundlastframe --;
}
if adjust_angle_timer > 1
{
	adjust_angle_timer --;
}

if onWall == true and landing == true
{
	if angle < 5 and angle > -5
	{
		landing_boost = 120;
		movespeed += 1;
	}
	landing = false;
}

#region SFX


if movespeed > 0
{
	if !audio_is_playing(sfx_rival_engineloop) and global.sfx == true
	{
		audio_play_sound(sfx_rival_engineloop, 10, true);
		audio_sound_pitch(sfx_rival_engineloop, 0.95);
	}
}
if movespeed == 0 or global.sfx == false
{
	if audio_is_playing(sfx_rival_engineloop)
	{
		audio_stop_sound(sfx_rival_engineloop);
	}
}


#endregion