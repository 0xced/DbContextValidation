# Bought https://thenounproject.com/icon/2931869/ for $2.99 in order to support the artist (and not require attribution)
# Then edited in Inkscape with Path -> Break Apart and changed colors
# rsvg-convert comes from MacPorts `librsvg`
png:
	rsvg-convert -w 400 -o icon.png icon.svg
	open -a Acorn icon.png
	# Using 256×256 to look nice on nuget.org, see https://github.com/NuGet/Home/issues/352#issuecomment-261096096
	# Image -> Resize Image… -> Withd = 200 / Height = 200 / Resolution = 144
	# Image -> Resize Canvas… -> Width = 256 / Height = 256
