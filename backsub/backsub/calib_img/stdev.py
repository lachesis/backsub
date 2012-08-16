#!/usr/bin/python2.7
import Image
import math
avgd = {}
sumsqd = {}
stdevd = {}
for x in xrange(0,640):
    for y in xrange(0,480):
        avgd[(x,y)] = [0,0,0]
        sumsqd[(x,y)] = [0,0,0]
        stdevd[(x,y)] = [0,0,0]

FRAMES = 20
for idx in xrange(0,FRAMES):
    print "Loading image {0}".format(idx+100)
    i = Image.open('output0{0}.png'.format(idx+100))
    pa = i.load()

    for x in xrange(0,640):
        for y in xrange(0,480):
            p = pa[x,y]
            for chan in xrange(3):
                avgd[(x,y)][chan] += (p[chan]/256.0 / FRAMES) 
                sumsqd[(x,y)][chan] += (p[chan]/256.0 ** 2) / FRAMES

avgi = i.copy()         
pa = avgi.load()
for x in xrange(0,640):
    for y in xrange(0,480):
        stdev = [0,0,0]
        for chan in xrange(3):
            stdev[chan] = math.sqrt(sumsqd[(x,y)][chan] - (avgd[(x,y)][chan]) ** 2)
        pa[x,y] = tuple([int(q*256.0) for q in stdev])
avgi.save('stdev.png')

