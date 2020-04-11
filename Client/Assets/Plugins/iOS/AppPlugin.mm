extern "C" {
    BOOL CanOpenApp(const char *url) {
        NSURL *nsUrl = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
        if([[UIApplication sharedApplication] canOpenURL:nsUrl]) {
            return true;
        }
        return false;
    }
}