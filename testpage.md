<html>
<head>
    <meta charset="utf-8" />
    <title>Gamedev Canvas Workshop</title>
    <style>
    	* { padding: 0; margin: 0; }
    	canvas { background: #eee; display: block; margin: 0 auto; }
    </style>
</head>
<body>

<canvas id="myCanvas" width="600" height="600"></canvas>
<button class="button" id="button01" onclick="playmusic()">開始播放</button>

<script>
	var num = 20;
	var canvas = document.getElementById("myCanvas");
	var ctx = canvas.getContext("2d");
	var isMouseDown = false;
	
	var current_type=1;
	
	var but = document.getElementById("button01");
	
	var patterns=[
      [[1, 0], [1, 1], [1, 2], [0, 3], [2, 3], [0, -1], [2, 3]], //head, neck, others
      [[3, 1], [2, 1], [0, 0], [0, 2], [1, 1], [1, 0], [3, 2]], 
      [[1, 3], [1, 2], [0, 0], [2, 0], [1, 1], [0, 1], [2, 3]], 
      [[0, 1], [1, 1], [2, 1], [3, 0], [3, 2], [-1, 0], [3, 2]], 
      [[3, 0], [2, 1], [0, 2], [1, 2], [1, 3], [1, -1], [3, 3]], 
      [[3, 3], [2, 2], [0, 1], [1, 0], [1, 1], [1, 1], [3, 3]], 
      [[0, 3], [1, 2], [2, 1], [3, 1], [2, 0], [-1, 1], [3, 3]], 
	  [[0, 0], [1, 1], [2, 2], [3, 2], [2, 3], [-1, -1], [3, 3]]];
	  
	function check_patteren(x, y, d, pat)
	{
		var ret = true;
		
		x-=pat[d][0];
		y-=pat[d][1];

		if (x<0 || x+pat[6][0]>num-1 || y<0 || y+pat[6][1]>num-1)
			return false;

		for (var i=1; i<5; i++)
			ret = ret & (table[x+pat[i-1][0]+(y+pat[i-1][1])*num]==table[x+pat[i][0]+(y+pat[i][1])*num]);

		return ret;
	}
	
	function inRange(x,d,u)
	{
	  return x>=d&&x<=u;
	}
	
	function cum(x, y, d, pat)  //to find how much player cums out
	{
		var 
			cum_num=0, 
			head_x=x+pat[0][0]-pat[d][0], //the location of the head of dick
			head_y=y+pat[0][1]-pat[d][1], 
			dick_type=table[head_x+head_y*num], 
			dx=pat[0][0]-pat[1][0], 
			dy=pat[0][1]-pat[1][1];

		for (head_x+=dx, head_y+=dy; inRange(head_x, 0, num-1)&&inRange(head_y, 0, num-1); head_x+=dx, head_y+=dy)
			if (table[head_x+head_y*num]!=0&&table[head_x+head_y*num]!=dick_type)
				cum_num++;
			else
				break;

		return cum_num;
	}
	
	function check_l(x,y)
	{
		var ret=false, patt_result=false;
		var cum_num=0;

		for (var i=0; i<8; i++)
			for (var dot=0; dot<5; dot++)
			{
				patt_result=check_patteren(x, y, dot, patterns[i]);
				ret=ret|patt_result;

				if (patt_result)
				{
				 cum_num+=cum(x, y, dot, patterns[i]);
			alert(cum_num);
				 }
			}
			
			if(ret)
			alert(cum_num);
		return ret?cum_num:-1;
	}
	
	var size=550.0,
		up=(canvas.width-size)/2.0,
		down=canvas.width-up,
		sizePerBlock=size/(num-1.0);
	
	var table = new Array(num*num);
	
	for(var i=0;i<table.length;i++)
		table[i]=0;
	draw_background();
	
	function drawGrid()
	{
		ctx.beginPath();
		for(var i=1;i<=num;i++)
		{
			ctx.moveTo(i*sizePerBlock,sizePerBlock);
			ctx.lineTo(i*sizePerBlock, down);
			
			ctx.moveTo(sizePerBlock,i*sizePerBlock);
			ctx.lineTo(down,i*sizePerBlock);
		}
		ctx.stroke();
	}
	
	function getGridLocation(x,y)
	{
		var pos=[0,0];
		pos[0] = Math.round(x/sizePerBlock) - 1;
		pos[1] = Math.round(y/sizePerBlock) - 1;
		
		return pos;
	}
	
	function getPostionFromGrid(x,y)
	{
		var pos=[0.0,0.0];
		
		pos[0]=(x+1)*sizePerBlock;
		pos[1]=(y+1)*sizePerBlock;
		
		return pos;
	}
	
	function canvasClicked(e)
	{
		var x;
		var y;
		if (e.pageX || e.pageY) { 
		  x = e.pageX;
		  y = e.pageY;
		}
		else { 
		  x = e.clientX + document.body.scrollLeft + document.documentElement.scrollLeft; 
		  y = e.clientY + document.body.scrollTop + document.documentElement.scrollTop; 
		} 
		x -= canvas.offsetLeft;
		y -= canvas.offsetTop;
		
		var loc = getGridLocation(x,y);
		table[loc[0]+loc[1]*num]=current_type;
		
		current_type = 3-current_type;
		
		check_l(loc[0],loc[1]);
	}
	
	function draw_background()
	{
					ctx.beginPath();
		ctx.rect(0,0,canvas.width,canvas.height);
		ctx.fillStyle="#dea44e";
		ctx.fill();
					ctx.closePath();
					
		drawGrid();
		draw_balls();
	}
	
	function draw_balls()
	{
		for(var x=0;x<num;x++)
			for(var y=0;y<num;y++)
				if(table[x+y*num]==1 || table[x+y*num]==2)
				{
					var pos = getPostionFromGrid(x,y);
				
					ctx.beginPath();
					ctx.arc(pos[0], pos[1],size/num/2,0,2*Math.PI);
					ctx.fillStyle = table[x+y*num]==1?"#000000":"#FFFFFF";
					ctx.fill();
					ctx.closePath();
				}
	}
	
	function drawPreview(e)
	{
		draw_background();
		
		var x;
		var y;
		if (e.pageX || e.pageY) { 
		  x = e.pageX;
		  y = e.pageY;
		}
		else { 
		  x = e.clientX + document.body.scrollLeft + document.documentElement.scrollLeft; 
		  y = e.clientY + document.body.scrollTop + document.documentElement.scrollTop; 
		} 
		x -= canvas.offsetLeft;
		y -= canvas.offsetTop;
		
		var loc = getGridLocation(x,y);
		var pos = getPostionFromGrid(loc[0],loc[1]);
		
			ctx.beginPath();
			ctx.arc(pos[0], pos[1],300.0/num,0,2*Math.PI);
			ctx.fillStyle = "#FF0000";
			ctx.fill();
			ctx.closePath();
	}
	
	canvas.addEventListener('click',canvasClicked);
	canvas.addEventListener('mousemove',drawPreview);
	canvas.addEventListener('mouseup',function(e){isMouseDown=false});
	canvas.addEventListener('mousedown',function(e){isMouseDown=true});
	
	//alert("test!");
</script>

</body>
</html>
