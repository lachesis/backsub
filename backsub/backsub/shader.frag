uniform sampler2D texture0;
uniform sampler2D texture1;
uniform int renderColor; 
void main()
{
	if(renderColor == 1)
	{
		gl_FragColor = gl_Color * texture2D(texture0, gl_TexCoord[0].xy);
	}
	else
	{
		gl_FragColor = texture2D(texture0, gl_TexCoord[0].xy);
	}
}