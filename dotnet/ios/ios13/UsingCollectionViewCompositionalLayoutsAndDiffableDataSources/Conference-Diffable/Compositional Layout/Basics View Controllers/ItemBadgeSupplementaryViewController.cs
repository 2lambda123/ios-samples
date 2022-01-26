﻿/*
See LICENSE folder for this sample’s licensing information.

Abstract:
Shows how to use NSCollectionLayoutSupplementaryItems to badge items
*/

namespace Conference_Diffable.CompositionalLayout.BasicsViewControllers;

public partial class ItemBadgeSupplementaryViewController : UIViewController {
	static readonly string badgeElementKind = nameof (badgeElementKind);
	class Section : NSObject, IEquatable<Section> {
		public static Section Main { get; } = new Section (1);

		public int Value { get; private set; }

		Section (int value) => Value = value;

		public static bool operator == (Section left, Section right)
		{
			if (ReferenceEquals (left, right))
				return true;

			if (left is null)
				return false;

			if (right is null)
				return false;

			return left.Equals (right);
		}

		public static bool operator != (Section left, Section right) => !(left == right);
		public override bool Equals (object obj) => this == (Section)obj;
		public bool Equals (Section? other) => Value == other?.Value;
		public override int GetHashCode () => HashCode.Combine (base.GetHashCode (), Value);
	}

	class Model : NSObject {
		public string Id { get; private set; }
		public string Title { get; private set; }
		public int BadgeCount { get; private set; }

		public Model (string title, int badgeCount)
		{
			Title = title;
			BadgeCount = badgeCount;

			Id = new NSUuid ().ToString ();
		}
	}

	UICollectionViewDiffableDataSource<Section, Model>? dataSource;
	UICollectionView? collectionView;

	public override void ViewDidLoad ()
	{
		base.ViewDidLoad ();
		// Perform any additional setup after loading the view, typically from a nib.

		NavigationItem.Title = "Item Badges";
		ConfigureHierarchy ();
		ConfigureDataSource ();
	}

	UICollectionViewLayout CreateLayout ()
	{
		var badgeAnchor = NSCollectionLayoutAnchor.CreateFromFractionalOffset (
			NSDirectionalRectEdge.Top | NSDirectionalRectEdge.Trailing, new CGPoint (.3, -.3));
		var badgeSize = NSCollectionLayoutSize.Create (NSCollectionLayoutDimension.CreateAbsolute (20),
			NSCollectionLayoutDimension.CreateAbsolute (20));
		var badge = NSCollectionLayoutSupplementaryItem.Create (badgeSize, badgeElementKind, badgeAnchor);

		var itemSize = NSCollectionLayoutSize.Create (NSCollectionLayoutDimension.CreateFractionalWidth (.25f),
			NSCollectionLayoutDimension.CreateFractionalHeight (1));
		var item = NSCollectionLayoutItem.Create (itemSize, badge);
		item.ContentInsets = new NSDirectionalEdgeInsets (5, 5, 5, 5);

		var groupSize = NSCollectionLayoutSize.Create (NSCollectionLayoutDimension.CreateFractionalWidth (1),
			NSCollectionLayoutDimension.CreateFractionalWidth (.2f));
		var group = NSCollectionLayoutGroup.CreateHorizontal (groupSize, item);

		var section = NSCollectionLayoutSection.Create (group);
		section.ContentInsets = new NSDirectionalEdgeInsets (20, 20, 20, 20);

		return new UICollectionViewCompositionalLayout (section);
	}

	void ConfigureHierarchy ()
	{
		if (View is null)
			throw new InvalidOperationException (nameof (View));

		collectionView = new UICollectionView (View.Bounds, CreateLayout ()) {
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
			BackgroundColor = UIColor.SystemBackgroundColor
		};
		collectionView.RegisterClassForCell (typeof (TextCell), TextCell.Key);
		collectionView.RegisterClassForSupplementaryView (typeof (BadgeSupplementaryView), new NSString (badgeElementKind), BadgeSupplementaryView.Key);
		View.AddSubview (collectionView);
	}

	void ConfigureDataSource ()
	{
		if (collectionView is null)
			throw new InvalidOperationException (nameof (collectionView));

		if (SupplementaryViewProviderHandler == null)
			throw new InvalidOperationException (nameof (SupplementaryViewProviderHandler));

		dataSource = new UICollectionViewDiffableDataSource<Section, Model> (collectionView, CellProviderHandler) {
			SupplementaryViewProvider = SupplementaryViewProviderHandler!
		};

		var random = new Random (DateTime.Now.Millisecond);
		var models = Enumerable.Range (0, 100).Select (i => new Model (i.ToString (), random.Next (0, 4))).ToArray ();

		// initial data
		var snapshot = new NSDiffableDataSourceSnapshot<Section, Model> ();
		snapshot.AppendSections (new [] { Section.Main });
		snapshot.AppendItems (models);
		dataSource.ApplySnapshot (snapshot, false);

		UICollectionViewCell CellProviderHandler (UICollectionView collectionView, NSIndexPath indexPath, NSObject obj)
		{
			var model = obj as Model;
			// Get a cell of the desired kind.
			if (collectionView.DequeueReusableCell (TextCell.Key, indexPath) is TextCell cell) {
				// Populate the cell with our item description.
				cell.Label.Text = model?.Title;
				cell.ContentView.BackgroundColor = CornflowerBlue;
				cell.Layer.BorderColor = UIColor.Black.CGColor;
				cell.Layer.BorderWidth = 1;
				cell.Layer.CornerRadius = 8;
				cell.Label.TextAlignment = UITextAlignment.Center;
				cell.Label.Font = UIFont.GetPreferredFontForTextStyle (UIFontTextStyle.Title1);

				// Return the cell.
				return cell;
			}
			throw new InvalidOperationException ("UICollectionViewCell");
		}

		UICollectionReusableView? SupplementaryViewProviderHandler (UICollectionView collectionView, string kind, NSIndexPath indexPath)
		{
			if (dataSource?.GetItemIdentifier (indexPath) is not Model model)
				return null;

			var hasBadgeCount = model.BadgeCount > 0;

			// Get a supplementary view of the desired kind.
			if (collectionView.DequeueReusableSupplementaryView (new NSString (kind), BadgeSupplementaryView.Key, indexPath) is BadgeSupplementaryView badgeView) {
				// Set the badge count as its label (and hide the view if the badge count is zero).
				badgeView.Label.Text = model.BadgeCount.ToString ();
				badgeView.Hidden = !hasBadgeCount;

				// Return the view.
				return badgeView;
			}
			throw new InvalidOperationException ("UICollectionReusableView");
		}
	}
}