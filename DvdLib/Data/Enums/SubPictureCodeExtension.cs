namespace DvdLib.Data.Enums;

public enum SubPictureCodeExtension : byte
{
    Unspecified = 0,
    Caption = 1,
    CaptionLarge = 2,
    CaptionForChildren = 3,
    
    ClosedCaption = 5,
    ClosedCaptionLarge = 6,
    ClosedCaptionForChildren = 7,
    
    ForcedCaption = 9,
    
    DirectorComments = 13,
    DirectorCommentsLarge = 14,
    DirectorCommentsForChildren = 15,
}