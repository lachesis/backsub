uniform sampler2D SumTx; // Sum of a/n; Same as mean
uniform sampler2D SumSqTx; // Sum of (a/n)^2
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
		gl_FragColor = samp(SumSqTx) + sq(samp(FrameTx))/NumFrames;
	//if(Mode == 2) // SumSqVar
	//	gl_FragColor = samp(SumSqTx) + sq(samp(FrameTx) - samp(SumTx));
	if(Mode == 3) // StdDev of scaled values
		gl_FragColor = sqrt(samp(SumSqTx) - sq(samp(SumTx)));
	//	gl_FragColor = sqrt(samp(SumSqTx) / NumFrames);
}