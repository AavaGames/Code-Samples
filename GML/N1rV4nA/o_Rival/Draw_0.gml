length = motionblur;

if (length > 0) {
    step = 3;
    dir = degtorad(180);
    px = cos(dir);
    py = -sin(dir);
 
    a = image_alpha/(length/step);
    if (a >= 1) {
        draw_sprite_ext(sprite_index,image_index,xx,yy,image_xscale,
            image_yscale,angle,image_blend,image_alpha);
        a /= 2;
    }
 
    for(i=length;i>=0;i-=step) {
        draw_sprite_ext(sprite_index,image_index,xx+(px*i),yy+(py*i),
            image_xscale,image_yscale,angle,image_blend,a);
    }
} else {    
    draw_sprite_ext(sprite_index,image_index,xx,yy,image_xscale,
        image_yscale,angle,image_blend,image_alpha);
}
draw_sprite_ext(sprite_index,image_index,xx,yy,1,1,angle,c_white,1)