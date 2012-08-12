uniform sampler2D SumTx; // Sum of a/n; Same as mean
uniform sampler2D SumSqsTx; // Sum of (a/n)^2
uniform sampler2D FrameTx;
uniform float NumFrames;

uniform int Mode;

#define samp(tex) texture2D(tex, gl_TexCoord[0].xy)
#define sq(v) pow(v, vec4(2.0f,2.0f,2.0f,2.0f))
 
void main()
{
	if(Mode == 0) // Passthrough
		gl_FragColor = samp(FrameTx);
	if(Mode == 1) // Sum of scaled values
		gl_FragColor = samp(SumTx) + samp(FrameTx) / NumFrames;
	if(Mode == 2) // SumSquares of scaled values
		gl_FragColor = samp(SumSqsTx) + sq(samp(FrameTx)/NumFrames);
	if(Mode == 3) // StdDev of scaled values
		gl_FragColor = sqrt(samp(SumSqsTx) - sq(samp(SumTx)))/(NumFrames-1.0f);
	//gl_FragColor = vec4(1,0,0,0);
}