uniform sampler2D texture0;
uniform sampler2D texture1;
void main()
{
	gl_FragColor = texture2D(texture0, gl_TexCoord[0].xy);
}