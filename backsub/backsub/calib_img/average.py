#!/usr/bin/python2.7
import Image
avgd = {}
for x in xrange(0,640):
    for y in xrange(0,480):
        avgd[(x,y)] = [0,0,0]
FRAMES = 60
for idx in xrange(100,100+FRAMES):
    print "Loading image {0}".format(idx)
    i = Image.open('output0{0}.png'.format(idx))

    pa = i.load()

    for x in xrange(0,640):
        for y in xrange(0,480):
            p = pa[x,y]
            avgd[(x,y)][0] += p[0] / float(FRAMES)
            avgd[(x,y)][1] += p[1] / float(FRAMES)
            avgd[(x,y)][2] += p[2] / float(FRAMES)

avgi = i.copy()         
pa = avgi.load()
for x in xrange(0,640):
    for y in xrange(0,480):
        a = avgd[(x,y)]
        pa[x,y] = tuple([int(q) for q in a])
avgi.save('average.png')

